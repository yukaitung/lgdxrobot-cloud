var MapDotNetObject = {};
var MapStage;
var MapLayer;
var MapBackground;
var MapBackgroundObj;
var MapEditorMode = 0;
const InitalScale = 3;

/*
 * Dotnet Functions
 */
function InitNavigationMap(dotNetObject) 
{
  MapDotNetObject = dotNetObject;
  const div = document.getElementById('navigation-map-container');
  const divRect = div.getBoundingClientRect();
  MapStage = new Konva.Stage({
    id: 'navigation-map-stage',
    container: 'navigation-map-canvas',
    width: divRect.width,
    height: divRect.height,
    draggable: true,
  });

  // Add map image
  const mapBackgroundImage = document.getElementById('navigation-map-image');
  MapBackgroundObj = new Image();
  MapBackgroundObj.src = mapBackgroundImage.src;
  MapBackgroundObj.onload = () => {
    let ctx = MapLayer.getContext()._context;
    ctx.imageSmoothingEnabled = false;
    MapBackground.fillPatternImage(MapBackgroundObj);
  };
  MapBackground = new Konva.Rect({
    x: 0,
    y: 0,
    width: mapBackgroundImage.width,
    height: mapBackgroundImage.height,
    draggable: false,
  });
  MapLayer = new Konva.Layer({
    offsetX: -divRect.width / 2 + mapBackgroundImage.width / 2,
    offsetY: -divRect.height / 2 + mapBackgroundImage.height / 2,
  });
  MapLayer.add(MapBackground);
  MapStage.add(MapLayer);

  // Mouse event on map
  if (document.getElementById('navigation-map-coordinate') != null)
  {
    MapStage.on('mousemove', () => {
      const p = document.getElementById('navigation-map-coordinate');
      if (!p) return;
      const pointer = MapStage.getPointerPosition();
      if (!pointer) return;

      // Transform stage coordinates to rect-local coordinates
      const localPos = MapBackground.getAbsoluteTransform().copy().invert().point(pointer);

      // Check if pointer is inside the rectangle bounds
      if (localPos.x >= 0 && localPos.x <= MapBackground.width() 
          && localPos.y >= 0 && localPos.y <= MapBackground.height()) 
      {
        let y = _internalToRobotPositionY(localPos.y);
        p.innerHTML = `X: ${_internalToRobotPositionX(localPos.x).toFixed(4)}m, Y: ${y.toFixed(4)}m`;
      } 
      else 
      {
        p.innerHTML = '';
      }
    });
  }
  MapStage.on('wheel', (e) => {
    e.evt.preventDefault();

    let direction = e.evt.deltaY > 0 ? -1 : 1;
    if (e.evt.ctrlKey) {
      // On trackpad
      direction = -direction;
    }
    if (direction > 0)
    {
      _internalMapZoom(1.1, true);
    }
    else
    {
      _internalMapZoom(-1.1, true);
    }
  });

  // Zoom the map
  MapStage.scale({ x: InitalScale, y: InitalScale });
  const newPos = {
    x: -divRect.width,
    y: -divRect.height,
  };
  MapStage.position(newPos);
  _internalRulerUpdate();

  // Resize when some events:
  window.addEventListener('resize', _internalOnResize);
  const sidebarButton = document.getElementById('sidebar-button');
  if (sidebarButton != null)
  {
    sidebarButton.addEventListener("click", () => {
      intervalId = setInterval(() => {
        _internalOnResize();
      }, 300);
    });
    sidebarButton.addEventListener("touchstart", () => {
      intervalId = setInterval(() => {
        _internalOnResize();
      }, 300);
    });
  }
  _internalOnResize();

  // Setup Plan
  const plan = new Konva.Line({
    id: 'currentRobotPlan',
    points: [],
    stroke: _internalGetCSSVariable('--tblr-blue'),
    strokeWidth: 1
  });
  MapLayer.add(plan);
}

/*
 * Zoom Functions
 */
function _internalOnResize()
{
  const div = document.getElementById('navigation-map');
  const container = document.getElementById('navigation-map-container');
  if (div == null || container == null)
  {
    return;
  }
  const divRect = div.getBoundingClientRect();
  const containerRect = container.getBoundingClientRect();
  MapStage.width(containerRect.width);
  MapStage.height(window.innerHeight - divRect.top);

  let ctx = MapLayer.getContext()._context;
  ctx.imageSmoothingEnabled = false;

  const sidebar = document.getElementById('navigation-map-sidebar');
  if (sidebar != null)
  {
    sidebar.style.maxHeight = window.innerHeight - divRect.top + 'px';
  }
}

function _internalRulerUpdate()
{
  const ruler = document.getElementById('navigation-map-ruler');
  var width = 1 / MAP_RESOLUTION * MapStage.scaleX();
  ruler.style.width = width + 'px';
}

function _internalMapZoom(scaleDiff, isWheel)
{
  if (scaleDiff == 0)
  {
    return;
  }

  const oldScale = MapStage.scaleX();
  if (oldScale <= InitalScale && scaleDiff < 0)
  {
    return;
  }
  if (oldScale >= 10 && scaleDiff > 0)
  {
    return;
  }

  const pointer = MapStage.getPointerPosition();
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
  const div = document.getElementById('navigation-map-container');
  if (div == null)
  {
    return;
  }
  const divRect = div.getBoundingClientRect();
  let pointTo = {
    x: (divRect.width / 2 - MapStage.x()) / oldScale,
    y: (divRect.height / 2 - MapStage.y()) / oldScale,
  };
  if (isWheel)
  {
    pointTo = {
      x: (pointer.x - MapStage.x()) / oldScale,
      y: (pointer.y - MapStage.y()) / oldScale,
    };
  }
  let newPos = {
    x: divRect.width / 2 - pointTo.x * newScale,
    y: divRect.height / 2 - pointTo.y * newScale,
  };
  if (isWheel)
  {
    newPos = {
      x: pointer.x - pointTo.x * newScale,
      y: pointer.y - pointTo.y * newScale,
    };
  }
  MapStage.position(newPos);
  _internalRulerUpdate();
}

function NavigationMapZoomIn()
{
  _internalMapZoom(1.1, false);
}

function NavigationMapZoomOut()
{
  _internalMapZoom(-1.1, false);
}

/*
 * Robot Functions
 */
function _internalToRobotPositionX(x)
{
  return x * MAP_RESOLUTION + MAP_ORIGIN_X;
}

function _internalToRobotPositionY(y)
{
  return (MapBackgroundObj.height - y) * MAP_RESOLUTION + MAP_ORIGIN_Y;
}
function _internalToMapX(x)
{
  return (-MAP_ORIGIN_X + x) / MAP_RESOLUTION;
}

function _internalToMapY(y)
{
  return MapBackgroundObj.height - ((-MAP_ORIGIN_Y + y) / MAP_RESOLUTION);
}

function _internalToMapRotation(rotation)
{
  return ((rotation + MAP_ORIGIN_ROTATION) * (180 / Math.PI) - 90);
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

function _internalGetCSSVariable(varName) {
  return getComputedStyle(document.documentElement).getPropertyValue(varName).trim();
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
      context.fillStyle = _internalGetCSSVariable('--tblr-gray-50');
      context.fill();
      context.strokeStyle = _internalGetCSSVariable('--tblr-gray-950');
      context.lineWidth = 1;
      context.stroke();
      },
    fill: _internalStringToHSVColor(robotId),
    stroke: _internalGetCSSVariable('--tblr-gray-950'),
    strokeWidth: 1,
    x: _internalToMapX(x),
    y: _internalToMapY(y),
  });
  robot.rotation(_internalToMapRotation(rotation));
  robot.on('click', function (e) {
    MapDotNetObject.invokeMethodAsync('HandleRobotSelect', e.target.id());
    ShowSidebar();
  });
  robot.on('touchstart', function (e) {
    MapDotNetObject.invokeMethodAsync('HandleRobotSelect', e.target.id());
    ShowSidebar();
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
  else
  {
    AddRobot(robotId, x, y, rotation);
  }
}

function UpdateRobotPlan(plan)
{
  for (let i = 0; i < plan.length; i += 2)
  {
    plan[i] = _internalToMapX(plan[i]);
    plan[i + 1] = _internalToMapY(plan[i + 1]);
  }
  const planLine = MapLayer.findOne('#currentRobotPlan');
  if (planLine != undefined)
  {
    planLine.points(plan);
  }
}

function ShowSidebar()
{
 const sidebar = document.getElementById('navigation-map-sidebar');
 if (sidebar != null)
 {
    sidebar.style.display = 'block';
    _internalOnResize();
 }
}

function HideSidebar()
{
 const sidebar = document.getElementById('navigation-map-sidebar');
 if (sidebar != null)
 {
    sidebar.style.display = 'none';
    _internalOnResize();
 }
}

/*
 * Map Editor
 */

function MapEditorSetMode(mode) { // assume is an integer
  MapEditorMode = mode;
}

function MapEditorAddWaypoints(waypoints) {
  for(let i = 0; i < waypoints.length; i++) {
    const w = new Konva.Circle({
      id: 'w-' + waypoints[i].id,
      x: _internalToMapX(waypoints[i].x),
      y: _internalToMapY(waypoints[i].y),
      radius: 4,
      fill: _internalGetCSSVariable('--tblr-blue-lt'),
      stroke: _internalGetCSSVariable('--tblr-blue'),
      strokeWidth: 1,
    });
    w.on('click', function (e) {
      let id = e.target.id();
      id = id.substring(2);
      switch (MapEditorMode) {
        case 0: // Normal
          // Show Waypoint Modal
          MapDotNetObject.invokeMethodAsync('HandleWaypointSelect', id);
          document.getElementById('waypointModalButton').click();
          break;
        case 1: // SingleWayTrafficFrom
        case 2: // SingleWayTrafficTo
        case 3: // BothWaysTrafficFrom
        case 4: // BothWaysTrafficTo
          // Continue traffic creation
          MapDotNetObject.invokeMethodAsync('HandleAddTraffic', id);
          break;
      }
    });
    MapLayer.add(w);
  }
}

function _internalGetConnectorPoints(from, to, isBothWaysTraffic) {
  const dx = to.x - from.x;
  const dy = to.y - from.y;
  let angle = Math.atan2(-dy, dx);

  const radius = 5;

  if (isBothWaysTraffic) {
    return [
      from.x + -radius * Math.cos(angle + Math.PI),
      from.y + radius * Math.sin(angle + Math.PI),
      to.x + -radius * Math.cos(angle),
      to.y + radius * Math.sin(angle),
    ];
  }
  // Make shorter line for arrow
  return [
    from.x + -radius * Math.cos(angle + Math.PI),
    from.y + radius * Math.sin(angle + Math.PI),
    to.x + -(radius + 1) * Math.cos(angle),
    to.y + (radius + 1) * Math.sin(angle),
  ];
}

function _internalHandleTrafficSelect(e) {
  if (MapEditorMode != 5) // DeleteTraffic mode
  {
    return;
  }

  let id = e.target.id();
  let obj = MapLayer.findOne('#' + id);
  obj.destroy();
  id = id.substring(2);
  MapDotNetObject.invokeMethodAsync('HandleDeleteTraffic', id);
}

function MapEditorAddTraffics(traffics) {
  for (var i = 0; i < traffics.length; i++) {
    const fromNode = MapLayer.findOne('#w-' + traffics[i].waypointFromId);
    const toNode = MapLayer.findOne('#w-' + traffics[i].waypointToId);
    const points = _internalGetConnectorPoints(
      fromNode.position(),
      toNode.position(),
      traffics[i].isBothWaysTraffic
    );
    if (traffics[i].isBothWaysTraffic) {
      const line = new Konva.Line({
        stroke: _internalGetCSSVariable('--tblr-blue'),
        strokeWidth: 1,
        id: 't-' + traffics[i].waypointFromId + '-' + traffics[i].waypointToId,
        points: points,
      });
      line.on('click', _internalHandleTrafficSelect);
      MapLayer.add(line);
    }
    else {
      const arrow = new Konva.Arrow({
        stroke: _internalGetCSSVariable('--tblr-blue'),
        fill: _internalGetCSSVariable('--tblr-blue'),
        strokeWidth: 1,
        id: 't-' + traffics[i].waypointFromId + '-' + traffics[i].waypointToId,
        points: points,
        pointerLength: 1,
        pointerWidth: 1,
      });
      arrow.on('click', _internalHandleTrafficSelect);
      MapLayer.add(arrow);
    }
  }
}

/*
 * SLAM
 */
function UpdateSlamMapSpecification(resolution, originX, originY, originRotation) 
{
  MAP_RESOLUTION = resolution;
  MAP_ORIGIN_X = originX;
  MAP_ORIGIN_Y = originY;
  MAP_ORIGIN_ROTATION = originRotation;
}

function UpdateSlamMap(width, height, mapData) 
{
  // Draw Map
  let canvas = document.getElementById("navigation-slam-map-data");
  canvas.width = width;
  canvas.height = height;
  let ctx = canvas.getContext("2d");
  ctx.reset();
  let iamgeData = ctx.createImageData(width, height);
  for (let row = 0; row < height; row++) {
    for (let col = 0; col < width; col++) {
      let index = col + ((height - row - 1) * width);
      let data = mapData[index];
      var value = 205;
      if (data === 100)
        value = 0;
      else if (data === 0)
        value = 255;

      var i = (col + (row * width)) * 4;
      iamgeData.data[i] = value;
      iamgeData.data[i + 1] = value;
      iamgeData.data[i + 2] = value;
      iamgeData.data[i + 3] = 255;
    }
  }
  ctx.putImageData(iamgeData, 0, 0);

  // Update Map
  MapBackgroundObj.src = canvas.toDataURL("image/png");
  MapBackground.width(width);
  MapBackground.height(height);
  _internalRulerUpdate();
}

function _internalSlamMapSetGoalStartHandler()
{
  const pointer = MapStage.getPointerPosition();
  if (!pointer) return;

  // Transform stage coordinates to rect-local coordinates
  const localPos = MapBackground.getAbsoluteTransform().copy().invert().point(pointer);

  // Check if pointer is inside the rectangle bounds
  if (localPos.x >= 0 && localPos.x <= MapBackground.width() 
      && localPos.y >= 0 && localPos.y <= MapBackground.height()) 
  {
    const arrow = new Konva.Arrow({
      stroke: _internalGetCSSVariable('--tblr-blue'),
      fill: _internalGetCSSVariable('--tblr-blue'),
      strokeWidth: 1,
      id: 'slamMapGoalArrow',
      points: [localPos.x, localPos.y, localPos.x, localPos.y],
      pointerLength: 1,
      pointerWidth: 1,
    });
    MapLayer.add(arrow);
  } 
}

function _internalSlamMapSetGoalMoveHandler()
{
  const arrow = MapLayer.findOne('#slamMapGoalArrow');
  if (arrow != undefined)
  {
    const pointer = MapStage.getPointerPosition();
    if (!pointer) return;

    // Transform stage coordinates to rect-local coordinates
    const localPos = MapBackground.getAbsoluteTransform().copy().invert().point(pointer);
    let path = arrow.points();
    path[2] = localPos.x;
    path[3] = localPos.y;
    arrow.points(path);
  }
}

function _internalGetAngleBetweenPoints(x1, y1, x2, y2) {
  const dx = x2 - x1;
  const dy = y2 - y1;
  return Math.atan2(dy, dx); // in radians
}

function _internalSlamMapSetGoalEndHandler()
{
  const arrow = MapLayer.findOne('#slamMapGoalArrow');
  if (arrow != undefined)
  {
    let path = arrow.points();
    let x = _internalToRobotPositionX(path[0]);
    let y = _internalToRobotPositionY(path[1]);
    let angle = _internalGetAngleBetweenPoints(path[0], path[1], path[2], path[3]);
    MapDotNetObject.invokeMethodAsync('HandleSetGoalSuccess', x, y, angle);
    SlamMapSetGoalStop();
  }
}

function SlamMapSetGoalStart() 
{
  const arrow = MapLayer.findOne('#slamMapGoalArrow');
  if (arrow != undefined)
  {
    arrow.destroy();
  }
  MapStage.draggable(false);
  MapStage.on('mousedown', _internalSlamMapSetGoalStartHandler);
  MapStage.on('touchstart', _internalSlamMapSetGoalStartHandler);
  MapStage.on('mousemove', _internalSlamMapSetGoalMoveHandler);
  MapStage.on('touchmove', _internalSlamMapSetGoalMoveHandler);
  MapStage.on('mouseup', _internalSlamMapSetGoalEndHandler);
  MapStage.on('touchend', _internalSlamMapSetGoalEndHandler);
}

function SlamMapSetGoalStop() 
{
  const arrow = MapLayer.findOne('#slamMapGoalArrow');
  if (arrow != undefined)
  {
    arrow.destroy();
  }
  MapStage.off('mousedown', _internalSlamMapSetGoalStartHandler);
  MapStage.off('touchstart', _internalSlamMapSetGoalStartHandler);
  MapStage.off('mousemove', _internalSlamMapSetGoalMoveHandler);
  MapStage.off('touchmove', _internalSlamMapSetGoalMoveHandler);
  MapStage.off('mouseup', _internalSlamMapSetGoalEndHandler);
  MapStage.off('touchend', _internalSlamMapSetGoalEndHandler);
  MapStage.draggable(true);
}