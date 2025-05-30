function CloseModal(modalId) {
  document.querySelector(modalId + ' .btn-close').click();
}

/*
.NET Methods
*/

var DotNetObject = {};
function InitDotNet(dotNetObject) {
  DotNetObject = dotNetObject;
  /*
  AdvancedSelectDict = {};
  AdvancedSelectBuffer = {};
  AdvancedSelectOptions = {};
  AdvancedSelectIsOnHold = {};
  AdvancedSelectTimer = {};*/
}

var ChangeRealmObject = {}
function InitChangeRealm(dotNetObject) {
  ChangeRealmObject = dotNetObject;
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
      valueField: "Id",
      labelField: "Name",
      searchField: "Name",
      controlInput: "<input>",
      onChange: AdvanceSelectEventHandler(elementId),
      render: {
        item: function (data, escape) {
          return ("<div>" + escape(data.Name) + "</div>");
        },
        option: function (data, escape) {
          return (
            "<div>" +
              "<div>" + escape(data.Name) + "</div>" +
              '<div class="text-secondary">ID: ' + escape(data.Id) + "</div>" +
            "</div>"
          );
        },
      },
    });
    AdvancedSelectDict[elementId + TS_CONTROL] = document.getElementById(elementId + TS_CONTROL);
    AdvancedSelectDict[elementId + TS_CONTROL].addEventListener("input", AdvanceSelectInput);
  }
}

function InitAdvancedSelectList(elementList, start, len) {
  start = parseInt(start);
  len = parseInt(len)
  if (!Array.isArray(elementList))
    return;
  for (let i = 0; i < elementList.length; i++) {
    for (let j = 0; j < len; j++) {
      let elementId = elementList[i] + (start + j).toString();
      if (document.getElementById(elementId) == null)
        break;
      InitAdvancedSelect(elementId);
    }
  }
}

var AdvanceSelectEventHandler = function(elementId) 
{
	return function() 
  {
    if (arguments[0] != undefined) 
    {
      if (arguments[0].length == 0)
      {
        if (elementId === "ChangeRealm")
        {
          ChangeRealmObject.invokeMethodAsync('HandleSelectChange', elementId, null, null);
        }
        else
        {
          DotNetObject.invokeMethodAsync('HandleSelectChange', elementId, null, null);
        }
      }
      else 
      {
        let optionId = arguments[0];
        if (Number.isInteger(arguments[0]))
        {
          // Id is not GUID
          optionId = parseInt(arguments[0]);
        }
        if (AdvancedSelectOptions[elementId] != undefined && AdvancedSelectOptions[elementId][optionId] != undefined) 
        {
          const optionName = AdvancedSelectOptions[elementId][optionId];
          if (elementId === "ChangeRealm")
          {
            ChangeRealmObject.invokeMethodAsync('HandleSelectChange', elementId, optionId, optionName);
          }
          else
          {
            DotNetObject.invokeMethodAsync('HandleSelectChange', elementId, optionId, optionName);
          }
        }
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
  if (idShort === "ChangeRealm")
  {
    ChangeRealmObject.invokeMethodAsync('HandlSelectSearch', idShort, AdvancedSelectBuffer[elementId]);
  }
  else
  {
    DotNetObject.invokeMethodAsync('HandlSelectSearch', idShort, AdvancedSelectBuffer[elementId]);
  }
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
      AdvancedSelectOptions[elementId][obj[i]["Id"]] = obj[i]["Name"];
    }
  }
}

function AdvanceControlExchange(elementList, indexA, indexB, isDelete = false) {
  if (!Array.isArray(elementList))
    return;
  for (let i = 0; i < elementList.length; i++) {
    if (AdvancedSelectDict[elementList[i] + indexA + TOM_SELECT_OBJECT] == undefined || AdvancedSelectDict[elementList[i] + indexB + TOM_SELECT_OBJECT] == undefined)
        continue;
    let idA = AdvancedSelectDict[elementList[i] + indexA + TOM_SELECT_OBJECT].getValue();
    let idB = AdvancedSelectDict[elementList[i] + indexB + TOM_SELECT_OBJECT].getValue();
    let objA = {Id: idA, Name: idA ? AdvancedSelectDict[elementList[i] + indexA + TOM_SELECT_OBJECT].options[idA].Name: ""};
    let objB = {Id: idB, Name: idB ? AdvancedSelectDict[elementList[i] + indexB + TOM_SELECT_OBJECT].options[idB].Name: ""};
    console.log(objA);
    console.log(objB);
      AdvancedSelectDict[elementList[i] + indexA + TOM_SELECT_OBJECT].clearOptions();
      AdvancedSelectDict[elementList[i] + indexA + TOM_SELECT_OBJECT].addItem(objB.Id, true);
      if (objB.Id.length > 0)
      {
        AdvancedSelectDict[elementList[i] + indexA + TOM_SELECT_OBJECT].addOption(objB);
      }
      AdvancedSelectDict[elementList[i] + indexA + TOM_SELECT_OBJECT].setValue(objB.Id);
    if (isDelete != true) {
      AdvancedSelectDict[elementList[i] + indexB + TOM_SELECT_OBJECT].clearOptions();
      AdvancedSelectDict[elementList[i] + indexB + TOM_SELECT_OBJECT].addItem(objA.Id, true);
      if (objA.Id.length > 0)
      {
        AdvancedSelectDict[elementList[i] + indexB + TOM_SELECT_OBJECT].addOption(objA);
      }
      AdvancedSelectDict[elementList[i] + indexB + TOM_SELECT_OBJECT].setValue(objA.Id);
    }
  }
}