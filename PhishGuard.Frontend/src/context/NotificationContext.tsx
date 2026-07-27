import { createContext, useState, useContext, type ReactNode } from 'react';
import { Snackbar, Alert } from '@mui/material';

type NotifySeverity = 'success' | 'error' | 'info' | 'warning';

interface NotificationContextType {
  showNotify: (message: string, type?: NotifySeverity) => void;
}

const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

export const NotificationProvider = ({ children }: { children: ReactNode }) => {
  const [notify, setNotify] = useState<{ open: boolean; message: string; type: NotifySeverity }>({
    open: false,
    message: '',
    type: 'success',
  });

  const showNotify = (message: string, type: NotifySeverity = 'success') => {
    setNotify({ open: true, message, type });
  };

  const handleClose = () => setNotify({ ...notify, open: false });

  return (
    <NotificationContext.Provider value={{ showNotify }}> 
      {children}
      <Snackbar 
        open={notify.open} 
        autoHideDuration={4000} 
        onClose={handleClose} 
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert onClose={handleClose} severity={notify.type} variant="filled" sx={{ width: '100%' }}>
          {notify.message}
        </Alert>
      </Snackbar>
    </NotificationContext.Provider>
  );
};

export const useNotify = () => {
  const context = useContext(NotificationContext);
  if (!context) throw new Error("useNotify deve ser usado dentro de um NotificationProvider");
  return context;
};