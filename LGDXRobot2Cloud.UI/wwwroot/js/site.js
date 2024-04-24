function CloseModal(modalId) {
  let myModalElement = document.getElementById(modalId);
  let modal = bootstrap.Modal.getInstance(myModalElement);
  modal.hide();
}

/*
.NET Methods
*/

var DotNetObject = {};
function InitDotNet(dotNetObject) {
  DotNetObject = dotNetObject;
  AdvancedSelectDict = {};
  AdvancedSelectBuffer = {};
  AdvancedSelectOptions = {};
  AdvancedSelectIsOnHold = {};
  AdvancedSelectTimer = {};
}

/*
Advanced Select methods
*/
const TS_CONTROL = "-ts-control";
const TOM_SELECT_OBJECT = "-tom-select-object";
var AdvancedSelectDict = {};
var AdvancedSelectBuffer = {};
var AdvancedSelectOptions = {};
var AdvancedSelectIsOnHold = {};
var AdvancedSelectTimer = {};
function InitAdvancedSelect(elementId) {
  if (document.getElementById(elementId + TS_CONTROL)) {
    // Tom select is initialised
    return;
  }
  if (AdvancedSelectDict[elementId] != undefined) {
    // The Blazor removed the DOM
    delete AdvancedSelectDict[elementId];
    AdvancedSelectDict[elementId + TS_CONTROL].removeEventListener("input", AdvanceSelectSearch);
    delete AdvancedSelectDict[elementId + TS_CONTROL];
    delete AdvancedSelectDict[elementId + TOM_SELECT_OBJECT];
  }
  if (window.TomSelect != undefined) {
    document.getElementById(elementId).style.display = "none"; 
    AdvancedSelectDict[elementId + TOM_SELECT_OBJECT] = new TomSelect(AdvancedSelectDict[elementId] = document.getElementById(elementId), {
      copyClassesToDropdown: false,
      valueField: "id",
      labelField: "name",
      searchField: "name",
      controlInput: "<input>",
      onChange: AdvanceSelectEventHandler(elementId),
      render: {
        item: function (data, escape) {
          return ("<div>" + escape(data.name) + "</div>");
        },
        option: function (data, escape) {
          return (
            "<div>" +
              "<div>" + escape(data.name) + "</div>" +
              '<div class="text-secondary">ID: ' + escape(data.id) + "</div>" +
            "</div>"
          );
        },
      },
    });
    AdvancedSelectDict[elementId + TS_CONTROL] = document.getElementById(elementId + TS_CONTROL);
    AdvancedSelectDict[elementId + TS_CONTROL].addEventListener("input", AdvanceSelectInput);
  }
}

var AdvanceSelectEventHandler = function(elementId) {
	return function() {
    if (arguments[0] != undefined) {
      if (arguments[0].length == 0)
        DotNetObject.invokeMethodAsync('HandleSelectChange', elementId, null, null);
      else {
        const optionId = parseInt(arguments[0]);
        const optionName = AdvancedSelectOptions[elementId][optionId] != undefined ? AdvancedSelectOptions[elementId][optionId] : "";
        DotNetObject.invokeMethodAsync('HandleSelectChange', elementId, optionId, optionName);
      }
    }
	};
};

function AdvanceSelectInput(e) {
  let elementId = e.target.id.toString();
  let search = e.target.value.toString();
  AdvancedSelectBuffer[elementId] = search;
  if (AdvancedSelectIsOnHold[elementId] == undefined || AdvancedSelectIsOnHold[elementId] == false) {
    AdvancedSelectIsOnHold[elementId] = true;
    AdvancedSelectTimer[elementId] = setTimeout(function(){ AdvanceSelectSearch(elementId); }, 200);
  }
}

function AdvanceSelectSearch(elementId) {
  var idShort = elementId.substring(0, (elementId.length - TS_CONTROL.length))
  DotNetObject.invokeMethodAsync('HandlSelectSearch', idShort, AdvancedSelectBuffer[elementId]);
  AdvancedSelectIsOnHold[elementId] = false;
}

function AdvanceSelectUpdate(elementId, result) {
  let obj = JSON.parse(result);
  AdvancedSelectOptions[elementId] = {};
  if (AdvancedSelectDict[elementId + TOM_SELECT_OBJECT] != undefined) {
    AdvancedSelectDict[elementId + TOM_SELECT_OBJECT].clearOptions();
    for (let i = 0; i < obj.length; i++) {
      // Update Tom Select
      AdvancedSelectDict[elementId + TOM_SELECT_OBJECT].addOption(obj[i]);
      // Update Buffer for Blazor
      AdvancedSelectOptions[elementId][obj[i]["id"]] = obj[i]["name"];
    }
  }
}