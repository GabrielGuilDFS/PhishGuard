import { Box } from '@mui/material';
import type { SxProps, Theme } from '@mui/material';
import type { ReactNode } from 'react';

interface PageContainerProps {
  children: ReactNode;
  sx?: SxProps<Theme>;
}

/**
 * Padrão GLOBAL de largura das telas administrativas.
 *
 * Equivalente MUI (sx) das classes Tailwind `mx-auto max-w-7xl min-w-[320px]`:
 * centraliza o conteúdo, limita a largura máxima (~1280px = max-w-7xl) para não se
 * espalhar em monitores ultrawide e garante uma largura mínima segura (320px).
 *
 * Implementado com `sx` (não com classes Tailwind) de propósito: o painel é 100% MUI e
 * assim nenhum estilo nativo de componente do MUI é sobrescrito/“corrompido”. O padding
 * horizontal responsivo (px-4 / sm:px-6 / lg:px-8) vive uma única vez no AdminLayout,
 * evitando padding duplicado.
 */
export default function PageContainer({ children, sx }: PageContainerProps) {
  return (
    <Box
      sx={[
        { width: '100%', maxWidth: 1280, minWidth: 320, mx: 'auto' },
        ...(Array.isArray(sx) ? sx : [sx]),
      ]}
    >
      {children}
    </Box>
  );
}
