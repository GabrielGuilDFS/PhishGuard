import { createContext, useState, useContext, type ReactNode } from 'react';
import { Snackbar, Alert } from '@mui/material';

interface NotificationContextType {
  showNotify: (message: string, type?: 'success' | 'error' | 'info' | 'warning') => void;
}

const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

export const NotificationProvider = ({ children }: { children: ReactNode }) => {
  const [notify, setNotify] = useState({ open: false, message: '', type: 'success' as any });

  const showNotify = (message: string, type: any = 'success') => {
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