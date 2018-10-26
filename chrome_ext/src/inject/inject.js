chrome.extension.sendMessage({}, function (response) {
  var readyStateCheckInterval = setInterval(function () {
    if (document.readyState === "complete") {
      clearInterval(readyStateCheckInterval);

      // ----------------------------------------------------------
      // This part of the script triggers when page is done loading
      console.log("Hello. This message was sent from scripts/inject.js");
      // ----------------------------------------------------------
    }
  }, 10);
});

chrome.runtime.onMessage.addListener(
  function (request, sender, sendResponse) {
    if (request.request == "question") {
      var question = document.getElementById("question");
      if (!question) {
        sendResponse({});
        return;
      }
      sendResponse({ question: question.getAttribute("data-questionid") });
    }
  });