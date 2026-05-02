mergeInto(LibraryManager.library, {
  setupFirebase: function (configJsonStr) {
    window.setupFirebase(UTF8ToString(configJsonStr));
  },

  sendFeedback: function (userName, comment) {
    window.sendFeedback(UTF8ToString(userName), UTF8ToString(comment));
  },

  // 大文字で呼ばれた時用（エラーを消すため）
  SendFeedbackJS: function (userName, comment) {
    window.sendFeedback(UTF8ToString(userName), UTF8ToString(comment));
  },

  OpenPromptJS: function (title, defaultText) {
    var result = window.prompt(UTF8ToString(title), UTF8ToString(defaultText));
    if (!result) return null;

    var bufferSize = lengthBytesUTF8(result) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(result, buffer, bufferSize);
    return buffer;
  }
});