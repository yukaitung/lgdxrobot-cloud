var CanvasObject;
var MapImage;
var RobotObjects;
var StartScale;
var TwoObject;
var WindowHeight = window.innerHeight;
var ZuiObject;
var MapDotNetObject = {};

/*
 * Dotnet Functions
 */
function InitNavigationMap(dotNetObject) 
{
  MapDotNetObject = dotNetObject;
  RobotObjects = {};

  // Init Two.js
  const div = document.getElementById("navigation-map-div");
  const divRect = div.getBoundingClientRect();
  CanvasObject = document.getElementById("navigation-map-canvas");
  TwoObject = new Two({
    autostart: true,
    domElement: CanvasObject,
    type: Two.Types.canvas,
    height: divRect.height,
    width: divRect.width
  });

  // Add map image
  MapImage = document.getElementById("navigation-map-image");
  const mapTexture = new Two.Texture(MapImage);
  const x = TwoObject.width * 0.5;
  const y = TwoObject.height * 0.5;
  let mapRect = TwoObject.makeRectangle(x, y, MapImage.width, MapImage.height);
  mapRect.fill = mapTexture;
  mapRect.noStroke();
  TwoObject.add(mapRect);

  // Add event listener
  TwoObject.renderer.domElement.addEventListener('click', _internalOnRobotClicked, false);
  TwoObject.renderer.domElement.addEventListener('touchstart', _internalOnRobotTouched, false);

  // Add Zui
  _internalAddZui();
  NavigationMapZoomReset();

  // Resize event listener
  window.addEventListener("resize", _internalOnResize);
  // disable image smoothing
  TwoObject.bind('update', function() {
    CanvasObject.getContext("2d").imageSmoothingEnabled = false;
  });
  
  function _internalOnRobotClicked(e)
  {
    const rect = CanvasObject.getBoundingClientRect();
    const x = e.clientX - (rect.left + window.scrollX);
    const y = e.clientY - (rect.top + window.scrollY);
    _internalOnRobotSelect(x, y);
  }
  function _internalOnRobotTouched(e)
  {
    const touch = e.touches[0];
    const rect = CanvasObject.getBoundingClientRect();
    const x = touch.clientX - (rect.left + window.scrollX);
    const y = touch.clientY - (rect.top + window.scrollY);
    _internalOnRobotSelect(x, y);
  }

  function _internalOnRobotSelect(x, y)
  {
    Object.entries(RobotObjects).forEach(([robotId, robotObject]) => {
      const rect = robotObject.getBoundingClientRect();
      if (x >= rect.left && x <= rect.right && y >= rect.top && y <= rect.bottom) {
        MapDotNetObject.invokeMethodAsync('HandleRobotSelect', robotId);
        document.getElementById("robotInformationPaneButton").click();
      }
    });
  }

  function _internalOnResize(e)
  {
    e.preventDefault;

    // Set Canvas size
    const newWindowHeight = window.innerHeight;
    const dy = newWindowHeight - WindowHeight;
    const div = document.getElementById("navigation-map-div");
    const divRect = div.getBoundingClientRect();
    TwoObject.renderer.setSize(divRect.width, divRect.height + (dy < 0 ? dy : 0));

    // Set Zui limits
    _internalSetScale(divRect.width, divRect.height, MapImage.width, MapImage.height);
    WindowHeight = newWindowHeight;
  }
}

function AddRobot(robotId, x, y, rotation)
{
  let teardrop = TwoObject.makeCurve(
    0, -6,   // Top center
    6, -1.5,  // Right curve control
    0, 6,  // Bottom tip
    0, 6,  // Bottom tip
    -6, -1.5,   // Left curve control
  );
  teardrop.fill = _internalGuidToHslColor(robotId);
  teardrop.stroke = 'none';

  // Add a circular highlight at the top
  let highlight = TwoObject.makeCircle(0, -2.5, 2);
  highlight.fill = 'white';
  highlight.opacity = 0.5;
  highlight.noStroke();

  let robot = TwoObject.makeGroup(teardrop, highlight);
  robot.translation.set(_internalToMapX(x), _internalToMapY(y));
  robot.rotation = _internalToMapRotation(rotation);

  TwoObject.add(robot);
  RobotObjects[robotId] = robot;
}

function MoveRobot(robotId, x, y, rotation)
{
  if (!robotId in RobotObjects)
    return;

  RobotObjects[robotId].translation.set(_internalToMapX(x), _internalToMapY(y));
  RobotObjects[robotId].rotation = _internalToMapRotation(rotation);
}

/*
 * Page Functions
 */
function NavigationMapZoomIn()
{
  _internalZoom(0.2);
}

function NavigationMapZoomOut()
{
  _internalZoom(-0.2);
}

function NavigationMapZoomReset()
{
  ZuiObject.reset();
  _internalSetScale()
  const div = document.getElementById("navigation-map-div");
  ZuiObject.zoomSet(StartScale, div.clientWidth / 2, div.clientHeight / 2);
}

/*
 * Private Functions
 */
function _internalToMapX(x)
{
  return TwoObject.width * 0.5 - MapImage.width / 2 - (MAP_ORIGIN_X - x) / MAP_RESOLUTION;
}

function _internalToMapY(y)
{
  return TwoObject.height * 0.5 + MapImage.height / 2 + (MAP_ORIGIN_Y - y) / MAP_RESOLUTION;
}

function _internalToMapRotation(rotation)
{
  return rotation - (Math.PI / 2) + MAP_ORIGIN_ROTATION;
}

function _internalZoom(scale)
{
  ZuiObject.zoomBy(scale, CanvasObject.clientWidth / 2, CanvasObject.clientHeight / 2);
}

function _internalSetScale()
{
  const div = document.getElementById("navigation-map-div");
  // Fit map to screen
  const canvasAspectRatio = div.clientWidth / div.clientHeight;
  const imageAspectRatio = MapImage.width / MapImage.height;
  if (imageAspectRatio > canvasAspectRatio) 
  {
    // Image is wider than canvas
    StartScale = div.clientWidth / MapImage.width;
  }
  else 
  {
    // Image is taller than canvas
    StartScale = div.clientHeight / MapImage.height;
  }
  ZuiObject.limits.scale.min = StartScale;
  ZuiObject.limits.scale.max = 10;
}

function _internalAddZui()
{
  // Init Zui
  const scene = TwoObject.renderer.scene;
  const domElement = TwoObject.renderer.domElement;
  ZuiObject = new Two.ZUI(scene);
  let mouse = new Two.Vector();
  let touches = {};
  let distance = 0;

  // Operations events and functions
  domElement.addEventListener('mousedown', zuiMouseDown, false);
  domElement.addEventListener('mousewheel', zuiMouseWheel, false);
  domElement.addEventListener('wheel', zuiMouseWheel, false);
  domElement.addEventListener('touchstart', zuiTouchStart, false);
  domElement.addEventListener('touchmove', zuiTouchMove, false);
  domElement.addEventListener('touchend', zuiTouchEnd, false);
  domElement.addEventListener('touchcancel', zuiTouchEnd, false);

  function zuiMouseDown(e)
  {
    mouse.x = e.clientX;
    mouse.y = e.clientY;
    const rect = scene.getBoundingClientRect();
    dragging = mouse.x > rect.left && mouse.x < rect.right
      && mouse.y > rect.top && mouse.y < rect.bottom;
    window.addEventListener('mousemove', zuiMouseMove, false);
    window.addEventListener('mouseup', zuiMouseUp, false);
  }

  function zuiMouseMove(e) 
  {
    const dx = e.clientX - mouse.x;
    const dy = e.clientY - mouse.y;
    ZuiObject.translateSurface(dx, dy);
    mouse.set(e.clientX, e.clientY);
  }

  function zuiMouseUp(e) 
  {
    window.removeEventListener('mousemove', zuiMouseMove, false);
    window.removeEventListener('mouseup', zuiMouseUp, false);
  }

  function zuiMouseWheel(e) 
  {
    var dy = (e.wheelDeltaY || - e.deltaY) / 1000;
    _internalZoom(dy);
  }

  function zuiTouchStart(e) 
  {
    e.preventDefault();
    switch (e.touches.length) 
    {
      case 2:
        zuiPinchStart(e);
        break;
      case 1:
        zuiPanStart(e)
        break;
    }
  }

  function zuiTouchMove(e) 
  {
    e.preventDefault();
    switch (e.touches.length) 
    {
      case 2:
        zuiPinchMove(e);
        break;
      case 1:
        zuiPanMove(e)
        break;
    }
  }

  function zuiTouchEnd(e)
  {
    e.preventDefault();
    touches = {};
    const touch = e.touches[0];
    if (touch) 
    {  
      // Pass through for panning after pinching
      mouse.x = touch.clientX;
      mouse.y = touch.clientY;
    }
  }

  function zuiPanStart(e) 
  {
    const touch = e.touches[0];
    mouse.x = touch.clientX;
    mouse.y = touch.clientY;
  }

  function zuiPanMove(e) 
  {
    const touch = e.touches[0];
    const dx = touch.clientX - mouse.x;
    const dy = touch.clientY - mouse.y;
    ZuiObject.translateSurface(dx, dy);
    mouse.set(touch.clientX, touch.clientY);
  }

  function zuiPinchStart(e) 
  {
    for (let i = 0; i < e.touches.length; i++) 
    {
      var touch = e.touches[i];
      touches[ touch.identifier ] = touch;
    }
    const a = touches[0];
    const b = touches[1];
    const dx = b.clientX - a.clientX;
    const dy = b.clientY - a.clientY;
    distance = Math.sqrt(dx * dx + dy * dy);
    mouse.x = dx / 2 + a.clientX;
    mouse.y = dy / 2 + a.clientY;
  }

  function zuiPinchMove(e) 
  {
    for (let i = 0; i < e.touches.length; i++) 
    {
      var touch = e.touches[i];
      touches[i] = touch;
    }
    const a = touches[0];
    const b = touches[1];
    const dx = b.clientX - a.clientX;
    const dy = b.clientY - a.clientY;
    const d = Math.sqrt(dx * dx + dy * dy);
    const delta = d - distance;
    if (delta <= 10 && delta >= -10)
    {
      _internalZoom(delta / 100);
    }
    distance = d;
  }
}

function _internalGuidToHslColor(guid) 
{
  // Remove dashes and take the last 6 characters from the GUID
  const colorPart = guid.replace(/-/g, '').substring(guid.length - 7, guid.length - 1);
  
  // If the GUID is shorter than 6 characters, fill with zeros
  const colorHex = colorPart.padEnd(6, '0');
  
  // Convert the hex color to RGB
  const r = parseInt(colorHex.substring(0, 2), 16);
  const g = parseInt(colorHex.substring(2, 4), 16);
  const b = parseInt(colorHex.substring(4, 6), 16);

  // Convert RGB to HSL
  const hsl = _internalRgbToHsl(r, g, b);

  // Set saturation to 100% and value to 80%
  hsl[1] = 1;  // 100% saturation
  hsl[2] = 0.8; // 80% value

  // Convert back to RGB and return the color
  const rgb = _internalHslToRgb(hsl[0], hsl[1], hsl[2]);
  
  // Return the color in hex format
  return _internalRgbToHex(rgb[0], rgb[1], rgb[2]);
}

function _internalRgbToHsl(r, g, b) {
  r /= 255;
  g /= 255;
  b /= 255;

  let max = Math.max(r, g, b), min = Math.min(r, g, b);
  let h, s, l = (max + min) / 2;

  if (max === min) {
      h = s = 0; // achromatic
  } else {
      const d = max - min;
      s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
      switch (max) {
          case r: h = (g - b) / d + (g < b ? 6 : 0); break;
          case g: h = (b - r) / d + 2; break;
          case b: h = (r - g) / d + 4; break;
      }
      h /= 6;
  }

  return [h, s, l];
}

function _internalHslToRgb(h, s, l) {
  let r, g, b;

  if (s === 0) {
      r = g = b = l; // achromatic
  } else {
      const hue2rgb = (p, q, t) => {
          if (t < 0) t += 1;
          if (t > 1) t -= 1;
          if (t < 1 / 6) return p + (q - p) * 6 * t;
          if (t < 1 / 2) return q;
          if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
          return p;
      };

      const q = l < 0.5 ? l * (1 + s) : l + s - l * s;
      const p = 2 * l - q;

      r = hue2rgb(p, q, h + 1 / 3);
      g = hue2rgb(p, q, h);
      b = hue2rgb(p, q, h - 1 / 3);
  }

  return [Math.round(r * 255), Math.round(g * 255), Math.round(b * 255)];
}

function _internalRgbToHex(r, g, b) {
  return `#${((1 << 24) | (r << 16) | (g << 8) | b).toString(16).slice(1).toUpperCase()}`;
}
