var CanvasObject;
var StartScale;
var TwoObject;
var WindowHeight = window.innerHeight;
var ZuiObject;
var MapImage;

/*
 * Dotnet Functions
 */
function InitNavigationMap() 
{
  // Init Two.js
  const div = document.getElementById("navigation-map-div");
  CanvasObject = document.getElementById("navigation-map-canvas");
  TwoObject = new Two({
    autostart: true,
    domElement: CanvasObject,
    type: Two.Types.canvas,
    height: div.clientHeight,
    width: div.clientWidth
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

  // Add robots
  var robot = TwoObject.makeCircle(_internalToMapX(0), _internalToMapY(0), 3);
  robot.fill = '#FF8000';
  robot.stroke = 'orangered';
  TwoObject.add(robot);

  // Add event listener
  TwoObject.renderer.domElement.addEventListener('click', _internalOnRobotClicked, false);
  TwoObject.renderer.domElement.addEventListener('touchstart', _internalOnRobotTouched, false);

  // Add Zui
  _internalAddZui();

  // Disabe image smoothing
  CanvasObject.getContext("2d").imageSmoothingEnabled = false;

  // Resize event listener
  window.addEventListener("resize", _internalOnResize);

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
    const rect = robot.getBoundingClientRect();
    if (x >= rect.left && x <= rect.right && y >= rect.top && y <= rect.bottom) {
      console.log("I am a robot!");
    }
  }

  function _internalOnResize(e)
  {
    e.preventDefault;

    // Set Canvas size
    const newWindowHeight = window.innerHeight;
    const dy = newWindowHeight - WindowHeight;
    const div = document.getElementById("navigation-map-div");
    TwoObject.renderer.setSize(div.clientWidth, div.clientHeight + (dy < 0 ? dy : 0));

    // Set Zui limits
    _internalSetScale(div.clientWidth, div.clientHeight, MapImage.width, MapImage.height);
    WindowHeight = newWindowHeight;

    // Disabe image smoothing
    CanvasObject.getContext("2d").imageSmoothingEnabled = false;
  }
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
  const canvas = document.getElementById("navigation-map-canvas");
  ZuiObject.zoomSet(StartScale, canvas.clientWidth / 2, canvas.clientHeight / 2);
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

function _internalZoom(scale)
{
  ZuiObject.zoomBy(scale, CanvasObject.clientWidth / 2, CanvasObject.clientHeight / 2);
}

function _internalSetScale()
{
  // Fit map to screen
  const canvasAspectRatio = CanvasObject.width / CanvasObject.height;
  const imageAspectRatio = MapImage.width / MapImage.height;
  if (imageAspectRatio > canvasAspectRatio) 
  {
    // Image is wider than canvas
    StartScale = CanvasObject.clientWidth / MapImage.width;
  }
  else 
  {
    // Image is taller than canvas
    StartScale = CanvasObject.clientHeight / MapImage.height;
  }
  ZuiObject.limits.scale.min = StartScale;
  ZuiObject.limits.scale.max = 10;
}

function _internalAddZui()
{
  // Init Zui
  const scene = TwoObject.renderer.scene;
  const domElement = TwoObject.renderer.domElement;
  if (ZuiObject != null)
  {
    // TODO: Find better way to handle this
    location.reload();
  }
  ZuiObject = new Two.ZUI(scene);
  let mouse = new Two.Vector();
  let touches = {};
  let distance = 0;

  _internalSetScale();
  ZuiObject.zoomSet(StartScale, CanvasObject.clientWidth / 2, CanvasObject.clientHeight / 2);

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