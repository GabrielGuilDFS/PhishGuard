// Matchers do jest-dom (toBeInTheDocument, toHaveValue, etc.) integrados ao Vitest.
import '@testing-library/jest-dom/vitest';

// Matchers de acessibilidade do vitest-axe (toHaveNoViolations) integrados ao Vitest.
// `extend-expect` registra os tipos; `expect.extend` registra a implementação.
import 'vitest-axe/extend-expect';
import { expect } from 'vitest';
import * as axeMatchers from 'vitest-axe/matchers';
expect.extend(axeMatchers);

// jsdom não implementa ResizeObserver (usado pelo MockupPanel em FeedbackTraining
// para escalar os iframes de mockup). Todo navegador real tem — polyfill inócuo.
if (typeof globalThis.ResizeObserver === 'undefined') {
  globalThis.ResizeObserver = class {
    observe() {}
    unobserve() {}
    disconnect() {}
  };
}
