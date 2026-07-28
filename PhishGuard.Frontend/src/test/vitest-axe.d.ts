// A augmentação embutida do vitest-axe mira o namespace legado `Vi` (jest-axe/vitest
// antigo), que o Vitest 3 NÃO usa mais para tipar a cadeia do `expect`. Por isso o
// `toHaveNoViolations` não aparecia no `tsc`. Aqui reexportamos os matchers dele para
// dentro do módulo `vitest` — o mesmo padrão do @testing-library/jest-dom/vitest.
import 'vitest';
import type { AxeMatchers } from 'vitest-axe/matchers';

declare module 'vitest' {
  interface Assertion<T = unknown> extends AxeMatchers {}
  interface AsymmetricMatchersContaining extends AxeMatchers {}
}
