function CloseModal(modalId) {
  var myModalElement = document.getElementById(modalId)
  var modal = bootstrap.Modal.getInstance(myModalElement)
  modal.hide();
}