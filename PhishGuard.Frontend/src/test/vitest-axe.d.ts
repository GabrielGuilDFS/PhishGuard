// A augmentação embutida do vitest-axe mira o namespace legado `Vi` (jest-axe/vitest
// antigo), que o Vitest 3 NÃO usa mais para tipar a cadeia do `expect`. Por isso o
// `toHaveNoViolations` não aparecia no `tsc`. Aqui reexportamos os matchers dele para
// dentro do módulo `vitest` — o mesmo padrão do @testing-library/jest-dom/vitest.
import 'vitest';
import type { AxeMatchers } from 'vitest-axe/matchers';

// As interfaces abaixo sao PURAMENTE declaration merging: precisam ser `interface`
// (type alias nao mescla), ficam vazias de proposito (todo o conteudo vem do
// `extends`) e o parametro `T` tem de existir mesmo sem uso, porque o TS exige lista
// de type parameters identica a da declaracao original do Vitest. Dai os disables.
/* eslint-disable @typescript-eslint/no-empty-object-type, @typescript-eslint/no-unused-vars */
declare module 'vitest' {
  interface Assertion<T = unknown> extends AxeMatchers {}
  interface AsymmetricMatchersContaining extends AxeMatchers {}
}
/* eslint-enable @typescript-eslint/no-empty-object-type, @typescript-eslint/no-unused-vars */
