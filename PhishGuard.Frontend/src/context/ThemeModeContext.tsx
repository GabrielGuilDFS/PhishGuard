import { createContext, useState, useContext, useMemo, useCallback, useEffect, useRef, type ReactNode } from 'react';
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { createAppTheme, type AppThemeMode } from '../theme';

// Preferência de tema do painel (dark/light). Persistida no navegador, na mesma
// convenção de chave do token JWT (`phishguard_*`).
const STORAGE_KEY = 'phishguard_theme_mode';
// Duração do fade na troca de tema (casada com o CSS `.theme-transition`).
const TRANSITION_MS = 1000;

interface ThemeModeContextType {
  /** Modo ativo do painel administrativo. */
  mode: AppThemeMode;
  /** Alterna entre light e dark. */
  toggleColorMode: () => void;
  /** Define o modo explicitamente. */
  setMode: (mode: AppThemeMode) => void;
}

const ThemeModeContext = createContext<ThemeModeContextType | undefined>(undefined);

// Modo de PRIMEIRA INTERAÇÃO = `light`. Só o valor salvo em localStorage muda isso;
// na sua ausência (ex.: usuário não logado abrindo a LandingHome) inicia em `light`.
function getInitialMode(): AppThemeMode {
  const saved = typeof localStorage !== 'undefined' ? localStorage.getItem(STORAGE_KEY) : null;
  return saved === 'dark' ? 'dark' : 'light';
}

export const ThemeModeProvider = ({ children }: { children: ReactNode }) => {
  const [mode, setModeState] = useState<AppThemeMode>(getInitialMode);
  const firstRender = useRef(true);

  // Persiste a preferência, reflete o modo em `<html data-theme>` (alterna as
  // variáveis CSS da paleta) e dispara o fade de 1s APENAS nas trocas — nunca no
  // primeiro paint (evita flash de transição ao carregar a página). Como a classe
  // sai depois de 1s, hovers e foco seguem com as transições curtas do MUI.
  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, mode);
    const root = document.documentElement;
    root.setAttribute('data-theme', mode);
    // Alias `.dark` — mantém o padrão de terceiros (Tailwind/libs) em sincronia.
    root.classList.toggle('dark', mode === 'dark');

    if (firstRender.current) {
      firstRender.current = false;
      return;
    }

    // prefers-reduced-motion: respeita quem pediu menos animação — troca instantânea.
    const reduzMovimento = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (reduzMovimento) return;

    root.classList.add('theme-transition');
    const timer = window.setTimeout(() => root.classList.remove('theme-transition'), TRANSITION_MS);
    return () => window.clearTimeout(timer);
  }, [mode]);

  const setMode = useCallback((next: AppThemeMode) => setModeState(next), []);
  const toggleColorMode = useCallback(
    () => setModeState((prev) => (prev === 'light' ? 'dark' : 'light')),
    []
  );

  // Recria o tema MUI só quando o modo muda (fábrica central em src/theme).
  const theme = useMemo(() => createAppTheme(mode), [mode]);
  const value = useMemo(() => ({ mode, toggleColorMode, setMode }), [mode, toggleColorMode, setMode]);

  return (
    <ThemeModeContext.Provider value={value}>
      <ThemeProvider theme={theme}>
        {/* CssBaseline é o único reset global (o preflight do Tailwind fica desligado);
            ao reagir ao tema, ele repinta background/texto padrão a cada troca de modo. */}
        <CssBaseline />
        {children}
      </ThemeProvider>
    </ThemeModeContext.Provider>
  );
};

export const useThemeMode = () => {
  const context = useContext(ThemeModeContext);
  if (!context) throw new Error('useThemeMode deve ser usado dentro de um ThemeModeProvider');
  return context;
};
