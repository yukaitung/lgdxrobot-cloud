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
}

function UnInitDotNet() {
  for (const [key, value] of Object.entries(AdvancedSelectTimer)) {
    clearTimeout(AdvancedSelectTimer[key]);
  }
}

/*
Advanced Select methods
*/
const TSCONTROL = "-ts-control";
const TOMSELECT = "-tomselect-control";
var AdvancedSelectDict = {};
var AdvancedSelectBuffer = {};
var AdvancedSelectIsOnHold = {};
var AdvancedSelectTimer = {};
function InitAdvancedSelect(elementId) {
  if (document.getElementById(elementId + TSCONTROL)) {
    // Tom select is initialised
    return;
  }
  if (AdvancedSelectDict[elementId] != undefined) {
    // The blazor has removed the dom
    delete AdvancedSelectDict[elementId];
    AdvancedSelectDict[elementId + TSCONTROL].removeEventListener("input", AdvanceSelectSearch);
    delete AdvancedSelectDict[elementId + TSCONTROL];
    delete AdvancedSelectDict[elementId + TOMSELECT];
  }
  if (window.TomSelect != undefined) {
    AdvancedSelectDict[elementId + TOMSELECT] = new TomSelect(AdvancedSelectDict[elementId] = document.getElementById(elementId), {
      copyClassesToDropdown: false,
      valueField: "id",
      labelField: "name",
      searchField: "name",
      controlInput: "<input>",
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
  }
    
  AdvancedSelectDict[elementId + TSCONTROL] = document.getElementById(elementId + TSCONTROL);
  AdvancedSelectDict[elementId + TSCONTROL].addEventListener("input", AdvanceSelectInput);
}

function AdvanceSelectInput(e) {
  let id = e.target.id.toString();
  let search = e.target.value.toString();
  AdvancedSelectBuffer[id] = search;
  if (AdvancedSelectIsOnHold[id] == undefined || AdvancedSelectIsOnHold[id] == false) {
    AdvancedSelectIsOnHold[id] = true;
    AdvancedSelectTimer[id] = setTimeout(function(){ AdvanceSelectSearch(id); }, 200);
  }
}

function AdvanceSelectSearch(id) {
  var idShort = id.substring(0, (id.length - TSCONTROL.length))
  DotNetObject.invokeMethodAsync('HandleApiKeySearch', idShort, AdvancedSelectBuffer[id]);
  AdvancedSelectIsOnHold[id] = false;
}

function AdvanceSelectUpdate(elementId, result) {
  let obj = JSON.parse(result);
  if (AdvancedSelectDict[elementId + TOMSELECT] != undefined) {
    AdvancedSelectDict[elementId + TOMSELECT].clearOptions();
    for (let i = 0; i < obj.length; i++) {
      AdvancedSelectDict[elementId + TOMSELECT].addOption(obj[i]);
    }
  }
}