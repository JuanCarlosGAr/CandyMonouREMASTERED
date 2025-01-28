mergeInto(LibraryManager.library, {

  GetURL: function () {
    var returnStr = window.location.href;
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },
  GetLocal: function () {
    var returnStr = localStorage.getItem("userId");
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },
  AddNumbers: function (x, y) {
    return x + y;
  },
  GetLocalEditable: function () {
    var returnStr = localStorage.getItem("editable");
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },
  GetLocalWindow: function () {
    var returnStr = window.localStorage.getItem("userId");
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },

});