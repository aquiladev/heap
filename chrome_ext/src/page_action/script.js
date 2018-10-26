chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
  chrome.tabs.sendMessage(tabs[0].id, { request: "question" }, function (response) {
    document.getElementById("questionId").innerHTML = response.question || 'NaN';
    if (response.question) {
      retrieveQ(response.question);
    }
  });
});

function retrieveQ(id) {
  chrome.storage.local.get(['storageConnection'], function (result) {
    var tableService = AzureStorage.Table.createTableService(result.storageConnection);
    tableService.retrieveEntity('questions', id, '', function (error, result, response) {
      console.log(error, result, response);

      var resultBlock = document.getElementById("result");
      var processBtn = document.getElementById("processBtn");
      if (error) {
        resultBlock.innerHTML = "<div style='color: red;'>" + JSON.stringify(error) + "</div>";
        processBtn.style.display = "block";
        processBtn.onclick = process(id);
        return;
      }
      resultBlock.innerHTML = result.Aylien._;
      processBtn.style.display = "none";
      retrieveBtn.style.display = "none";
    });
  });
}

function process(id) {
  return function () {
    chrome.storage.local.get(['storageConnection'], function (result) {
      var queueService = AzureStorage.Queue.createQueueService(result.storageConnection);
      queueService.createMessage('questions', window.btoa(id), function (error, results, response) {
        console.log(error, results, response);

        var resultBlock = document.getElementById("result");
        var processBtn = document.getElementById("processBtn");
        var retrieveBtn = document.getElementById("retrieveBtn");
        if (error) {
          resultBlock.innerHTML = "<div style='color: red;'>" + JSON.stringify(error) + "</div>";
          return;
        }
        processBtn.style.display = "none";
        retrieveBtn.style.display = "block";
        retrieveBtn.onclick = retrieve(id);
      });
    });
  };
}

function retrieve(id) {
  return function () {
    return retrieveQ(id);
  };
}