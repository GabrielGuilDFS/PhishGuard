import Box from '@mui/material/Box';
import phishGuardMark from '../assets/phishguard-mark.png';

type PhishGuardMarkProps = {
  size?: number;
  marginRight?: number;
  className?: string;
};

/** Marca compacta do PhishGuard para contextos em que o nome já está visível. */
export default function PhishGuardMark({
  size = 30,
  marginRight = 0,
  className,
}: PhishGuardMarkProps) {
  return (
    <Box
      component="img"
      src={phishGuardMark}
      alt=""
      aria-hidden="true"
      className={className}
      sx={{
        display: 'block',
        width: size,
        height: size,
        objectFit: 'contain',
        flexShrink: 0,
        mr: marginRight,
      }}
    />
  );
}
