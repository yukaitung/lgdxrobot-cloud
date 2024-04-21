function CloseModal(modalId) {
  let myModalElement = document.getElementById(modalId);
  let modal = bootstrap.Modal.getInstance(myModalElement);
  modal.hide();
}

var AdvancedSelectDict = {};
function InitAdvancedSelect(elementId) {
  if (AdvancedSelectDict[elementId])
    delete AdvancedSelectDict[elementId];
  window.TomSelect &&
    new TomSelect(document.getElementById(elementId), {
      copyClassesToDropdown: false,
      controlInput: "<input>",
      render: {
        item: function (data, escape) {
          return "<div>" + escape(data.text) + "</div>";
        },
        option: function (data, escape) {
          return "<div>" + escape(data.text) + "</div>";
        },
      },
    });
}
