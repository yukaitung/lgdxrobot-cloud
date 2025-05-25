var WindowHeight = window.innerHeight;
var MapDotNetObject = {};
var MapStage;
var MapLayer

/*
 * Dotnet Functions
 */
function InitNavigationMap(dotNetObject) 
{
  MapDotNetObject = dotNetObject;
  const div = document.getElementById('navigation-map-div');
  const divRect = div.getBoundingClientRect();
  MapStage = new Konva.Stage({
    container: 'navigation-map-canvas',
    width: divRect.width,
    height: divRect.height,
    draggable: true,
  });

  // Add map image
  const mapBackgroundImage = document.getElementById('navigation-map-image');
  const mapBackgroundObj = new Image();
  mapBackgroundObj.src = mapBackgroundImage.src;
  const mapBackground = new Konva.Rect({
    x: 0,
    y: 0,
    width: mapBackgroundImage.width,
    height: mapBackgroundImage.height,
    draggable: false,
  });
  mapBackgroundObj.onload = () => {
    let ctx = MapLayer.getContext()._context;
    ctx.imageSmoothingEnabled = false;
    mapBackground.fillPatternImage(mapBackgroundObj);
  };
  MapLayer = new Konva.Layer({
    offsetX: -divRect.width / 2 + mapBackgroundImage.width / 2,
    offsetY: -divRect.height / 2 + mapBackgroundImage.height / 2,
  });
  MapStage.add(MapLayer);
  MapLayer.add(mapBackground);

  function _internalOnResize()
  {
    const newWindowHeight = window.innerHeight;
    const dy = newWindowHeight - WindowHeight;
    const div = document.getElementById('navigation-map-div');
    const divRect = div.getBoundingClientRect();
    MapStage.width(divRect.width);
    MapStage.height(divRect.height + (dy < 0 ? dy : 0));
    WindowHeight = newWindowHeight;

    let ctx = MapLayer.getContext()._context;
    ctx.imageSmoothingEnabled = false;
  }

  const scaleBy = 1.1;
  MapStage.on('wheel', (e) => {
    e.evt.preventDefault();

    const oldScale = MapStage.scaleX();
    const pointer = MapStage.getPointerPosition();
    const mousePointTo = {
      x: (pointer.x - MapStage.x()) / oldScale,
      y: (pointer.y - MapStage.y()) / oldScale,
    };

    let direction = e.evt.deltaY > 0 ? -1 : 1;
    if (e.evt.ctrlKey) {
      // On trackpad
      direction = -direction;
    }

    const newScale = direction > 0 ? oldScale * scaleBy : oldScale / scaleBy;
    MapStage.scale({ x: newScale, y: newScale });
    const newPos = {
      x: pointer.x - mousePointTo.x * newScale,
      y: pointer.y - mousePointTo.y * newScale,
    };
    MapStage.position(newPos);
  });

  // Zoom the map
  const initalScale = 3;
  MapStage.scale({ x: initalScale, y: initalScale });
  const newPos = {
    x: -divRect.width,
    y: -divRect.height,
  };
  MapStage.position(newPos);
  window.addEventListener('resize', _internalOnResize);
}

/*
 * Zoom Functions
 */

function _internalMapZoom(scaleDiff)
{
  if (scaleDiff == 0)
  {
    return;
  }

  const oldScale = MapStage.scaleX();
  let newScale = 1;
  if (scaleDiff > 0)
  {
    newScale = oldScale * scaleDiff;
  }
  else if (scaleDiff < 0)
  {
    newScale = oldScale / -scaleDiff;
  }
  MapStage.scale({ x: newScale, y: newScale });
  const div = document.getElementById('navigation-map-div');
  const divRect = div.getBoundingClientRect();
  const pointTo = {
    x: (divRect.width / 2 - MapStage.x()) / oldScale,
    y: (divRect.height / 2 - MapStage.y()) / oldScale,
  };
  const newPos = {
    x: divRect.width / 2 - pointTo.x * newScale,
    y: divRect.height / 2 - pointTo.y * newScale,
  };
  MapStage.position(newPos);
}

function NavigationMapZoomIn()
{
  _internalMapZoom(1.1);
}

function NavigationMapZoomOut()
{
  _internalMapZoom(-1.1);
}

function NavigationMapZoomReset()
{
  const div = document.getElementById('navigation-map-div');
  const divRect = div.getBoundingClientRect();
  MapStage.x(-divRect.width);
  MapStage.y(-divRect.height);
  const initalScale = 3;
  MapStage.scale({ x: initalScale, y: initalScale });
  const newPos = {
    x: -divRect.width,
    y: -divRect.height,
  };
  MapStage.position(newPos);
}

/*
 * Robot Functions
 */
function _internalToMapX(x)
{
  return (-MAP_ORIGIN_X + x) / MAP_RESOLUTION;
}

function _internalToMapY(y)
{
  const mapBackgroundImage = document.getElementById('navigation-map-image');
  return mapBackgroundImage.height - ((-MAP_ORIGIN_Y + y) / MAP_RESOLUTION);
}

function _internalToMapRotation(rotation)
{
  return (rotation + MAP_ORIGIN_ROTATION * (Math.PI / 2)) - 90;
}

function _internalStringToHSVColor(str) 
{
  const lastSix = str.slice(-6);
  let seed = 0;
  for (let i = 0; i < lastSix.length; i++) {
    seed += lastSix.charCodeAt(i) * (i + 1);
  }
  const hue = seed % 360;
  const saturation = 0.6;
  const value = 0.9;

  const rgb = _internalHsvToRgb(hue, saturation, value);
  // Convert RGB to Hex
  const hex = rgb.map(c => Math.round(c * 255).toString(16).padStart(2, '0')).join('');
  return `#${hex}`;
}

function _internalHsvToRgb(h, s, v) 
{
  const c = v * s;
  const x = c * (1 - Math.abs((h / 60) % 2 - 1));
  const m = v - c;

  let [r, g, b] = [0, 0, 0];
  if (h < 60)       [r, g, b] = [c, x, 0];
  else if (h < 120) [r, g, b] = [x, c, 0];
  else if (h < 180) [r, g, b] = [0, c, x];
  else if (h < 240) [r, g, b] = [0, x, c];
  else if (h < 300) [r, g, b] = [x, 0, c];
  else              [r, g, b] = [c, 0, x];

  return [r + m, g + m, b + m];
}

function AddRobot(robotId, x, y, rotation)
{
  let r = MapLayer.findOne('#' + robotId);
  if (r != undefined)
  {
    return;
  }

  const robot = new Konva.Shape({
    id: robotId,
    sceneFunc: function (context, shape) {
      const radius = 5;     
      const tailLength = 10;      
      const controlScale = 1.05; 
      const dotRadius = 2; 

      // Draw pin body
      context.beginPath();
      context.arc(0, 0, radius, 0, Math.PI, true);
      context.bezierCurveTo(
        -radius * controlScale, radius * controlScale,
        -3, tailLength * 0.7,
        0, tailLength
      );
      context.bezierCurveTo(
        3, tailLength * 0.7,
        radius * controlScale, radius * controlScale,
        radius, 0
      );
      context.closePath();
      context.fillStrokeShape(shape);

      // Draw inner circle
      context.beginPath();
      context.arc(0, 0, dotRadius, 0, Math.PI * 2, false);
      context.fillStyle = 'white';
      context.fill();
      context.strokeStyle = 'black';
      context.lineWidth = 1;
      context.stroke();
      },
    fill: _internalStringToHSVColor(robotId),
    stroke: 'black',
    strokeWidth: 1,
    x: _internalToMapX(x),
    y: _internalToMapY(y),
  });
  robot.rotate(_internalToMapRotation(rotation));
  robot.on('click', function (e) {
    MapDotNetObject.invokeMethodAsync('HandleRobotSelect', e.target.id());
    document.getElementById("robotDataOffcanvasButton").click();
  });
  MapLayer.add(robot);
}

function MoveRobot(robotId, x, y, rotation)
{
  let robot = MapLayer.findOne('#' + robotId);
  if (robot != undefined)
  {
    robot.x(_internalToMapX(x));
    robot.y(_internalToMapY(y));
    robot.rotation(_internalToMapRotation(rotation));
  }
}
