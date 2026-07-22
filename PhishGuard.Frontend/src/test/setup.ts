// Matchers do jest-dom (toBeInTheDocument, toHaveValue, etc.) integrados ao Vitest.
import '@testing-library/jest-dom/vitest';

// jsdom não implementa ResizeObserver (usado pelo MockupPanel em FeedbackTraining
// para escalar os iframes de mockup). Todo navegador real tem — polyfill inócuo.
if (typeof globalThis.ResizeObserver === 'undefined') {
  globalThis.ResizeObserver = class {
    observe() {}
    unobserve() {}
    disconnect() {}
  };
}
