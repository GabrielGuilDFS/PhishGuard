import { render } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import PhishGuardMark from './PhishGuardMark';

describe('PhishGuardMark', () => {
  it('renderiza a marca oficial como imagem decorativa no tamanho solicitado', () => {
    const { container } = render(<PhishGuardMark size={36} />);
    const mark = container.querySelector('img');

    expect(mark).not.toBeNull();
    expect(mark).toHaveAttribute('src', expect.stringContaining('phishguard-mark'));
    expect(mark).toHaveAttribute('alt', '');
    expect(mark).toHaveAttribute('aria-hidden', 'true');
    expect(mark).toHaveStyle({ width: '36px', height: '36px' });
  });
});
