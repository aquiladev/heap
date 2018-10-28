// Saves options to chrome.storage
function save_options() {
  var storageConnection = document.getElementById('storage_connection').value;
  chrome.storage.local.set({
    storageConnection: storageConnection
  }, function () {
    // Update status to let user know options were saved.
    var status = document.getElementById('status');
    status.textContent = 'Options saved!';
    setTimeout(function () {
      status.textContent = '';
    }, 750);
  });
}

// Restores select box and checkbox state using the preferences
// stored in chrome.storage.
function restore_options() {
  chrome.storage.local.get({
    storageConnection: '',
  }, function (items) {
    document.getElementById('storage_connection').value = items.storageConnection;
  });
}

document.addEventListener('DOMContentLoaded', restore_options);
document.getElementById('save').addEventListener('click', save_options);