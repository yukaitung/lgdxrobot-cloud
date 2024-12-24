var canvas;
var startScale;
var two;
var windowHeight = window.innerHeight;
var zui;

/*
 * Dotnet Functions
 */
function InitNavigationMap() 
{
  // Init Two.js
  let div = document.getElementById("navigation-map-div");
  canvas = document.getElementById("navigation-map-canvas");
  two = new Two({
    autostart: true,
    domElement: canvas,
    type: Two.Types.canvas,
    height: div.clientHeight,
    width: div.clientWidth
  });

  // Add map image
  let mapImage = document.getElementById("navigation-map-image");
  let mapTexture = new Two.Texture(mapImage);
  let x = two.width * 0.5;
  let y = two.height * 0.5;
  let imageWidth = mapImage.width;
  let imageHeight = mapImage.height;
  let mapRect = two.makeRectangle(x, y, imageWidth, imageHeight);
  mapRect.fill = mapTexture;
  mapRect.noStroke();
  two.add(mapRect);

  // Add robots
  var robot = two.makeCircle(x, y, 3);
  robot.fill = '#FF8000';
  robot.stroke = 'orangered';
  two.add(robot);

  // Add event listener
  two.renderer.domElement.addEventListener('click', _internalOnRobotSelect, false);
  two.renderer.domElement.addEventListener('touchstart', _internalOnRobotTouched, false);

  // Add ZUI
  _internalAddZui(canvas.width, canvas.height, imageWidth, imageHeight);

  // Disabe image smoothing
  canvas.getContext("2d").imageSmoothingEnabled = false;

  // Resize event listener
  window.addEventListener("resize", _internalOnResize);

  function _internalOnRobotSelect(e)
  {
    var rect = robot.getBoundingClientRect();
    var rect2 = canvas.getBoundingClientRect();
    var x = e.clientX - (rect2.left + window.scrollX);
    var y = e.clientY - (rect2.top + window.scrollY);
    console.log(x, y);
    if (x >= rect.left && x <= rect.right && y >= rect.top && y <= rect.bottom) {
      // your custom function
      console.log("I am a robot!");
    }
  }
  function _internalOnRobotTouched(e)
  {
    var touch = e.touches[0];
    var rect = robot.getBoundingClientRect();
    var rect2 = canvas.getBoundingClientRect();
    var x = touch.clientX - (rect2.left + window.scrollX);
    var y = touch.clientY - (rect2.top + window.scrollY);
    console.log(x, y);
    if (x >= rect.left && x <= rect.right && y >= rect.top && y <= rect.bottom) {
      // your custom function
      console.log("I am a robot!");
    }
  }

  function _internalOnResize(e)
  {
    e.preventDefault;

    // Set Canvas size
    const newWindowHeight = window.innerHeight;
    const dy = newWindowHeight - windowHeight;
    const div = document.getElementById("navigation-map-div");
    two.renderer.setSize(div.clientWidth, div.clientHeight + (dy < 0 ? dy : 0));

    // Set ZUI limits
    const mapImage = document.getElementById("navigation-map-image");
    _internalSetScale(div.clientWidth, div.clientHeight, mapImage.width, mapImage.height);

    windowHeight = newWindowHeight;

    // Disabe image smoothing
    canvas.getContext("2d").imageSmoothingEnabled = false;
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
  zui.reset();
  _internalSetScale()
  let canvas = document.getElementById("navigation-map-canvas");
  zui.zoomSet(startScale, canvas.clientWidth / 2, canvas.clientHeight / 2);
}

/*
 * Private Functions
 */
function _internalZoom(scale)
{
  zui.zoomBy(scale, canvas.clientWidth / 2, canvas.clientHeight / 2);
}

function _internalSetScale()
{
  let canvas = document.getElementById("navigation-map-canvas");
  let mapImage = document.getElementById("navigation-map-image");

  // Fit map to screen
  const canvasAspectRatio = canvas.width / canvas.height;
  const imageAspectRatio = mapImage.width / mapImage.height;
  if (imageAspectRatio > canvasAspectRatio) 
  {
    // Image is wider than canvas
    startScale = canvas.clientWidth / mapImage.width;
  }
  else 
  {
    // Image is taller than canvas
    startScale = canvas.clientHeight / mapImage.height;
  }
  zui.limits.scale.min = startScale;
  zui.limits.scale.max = 10;
}

function _internalAddZui()
{
  // Init ZUI
  const scene = two.renderer.scene;
  const domElement = two.renderer.domElement;
  if (zui != null)
  {
    // TODO: Find better way to handle this
    location.reload();
  }
  zui = new Two.ZUI(scene);
  let mouse = new Two.Vector();
  let touches = {};
  let distance = 0;

  _internalSetScale();
  let canvas = document.getElementById("navigation-map-canvas");
  zui.zoomSet(startScale, canvas.clientWidth / 2, canvas.clientHeight / 2);

  domElement.addEventListener('mousedown', mousedown, false);
  domElement.addEventListener('mousewheel', mousewheel, false);
  domElement.addEventListener('wheel', mousewheel, false);

  domElement.addEventListener('touchstart', touchstart, false);
  domElement.addEventListener('touchmove', touchmove, false);
  domElement.addEventListener('touchend', touchend, false);
  domElement.addEventListener('touchcancel', touchend, false);

  function mousedown(e) 
  {
    mouse.x = e.clientX;
    mouse.y = e.clientY;
    var rect = scene.getBoundingClientRect();
    dragging = mouse.x > rect.left && mouse.x < rect.right
      && mouse.y > rect.top && mouse.y < rect.bottom;
    window.addEventListener('mousemove', mousemove, false);
    window.addEventListener('mouseup', mouseup, false);
  }

  function mousemove(e) 
  {
    var dx = e.clientX - mouse.x;
    var dy = e.clientY - mouse.y;
    zui.translateSurface(dx, dy);
    mouse.set(e.clientX, e.clientY);
  }

  function mouseup(e) 
  {
    window.removeEventListener('mousemove', mousemove, false);
    window.removeEventListener('mouseup', mouseup, false);
  }

  function mousewheel(e) 
  {
    var dy = (e.wheelDeltaY || - e.deltaY) / 1000;
    _internalZoom(dy);
  }

  function touchstart(e) 
  {
    e.preventDefault();
    switch (e.touches.length) 
    {
      case 2:
        pinchstart(e);
        break;
      case 1:
        panstart(e)
        break;
    }
  }

  function touchmove(e) 
  {
    e.preventDefault();
    switch (e.touches.length) 
    {
      case 2:
        pinchmove(e);
        break;
      case 1:
        panmove(e)
        break;
    }
  }

  function touchend(e)
  {
    e.preventDefault();
    touches = {};
    var touch = e.touches[0];
    if (touch) 
    {  
      // Pass through for panning after pinching
      mouse.x = touch.clientX;
      mouse.y = touch.clientY;
    }
  }

  function panstart(e) 
  {
    var touch = e.touches[0];
    mouse.x = touch.clientX;
    mouse.y = touch.clientY;
  }

  function panmove(e) 
  {
    var touch = e.touches[0];
    var dx = touch.clientX - mouse.x;
    var dy = touch.clientY - mouse.y;
    zui.translateSurface(dx, dy);
    mouse.set(touch.clientX, touch.clientY);
  }

  function pinchstart(e) 
  {
    for (var i = 0; i < e.touches.length; i++) 
    {
      var touch = e.touches[i];
      touches[ touch.identifier ] = touch;
    }
    var a = touches[0];
    var b = touches[1];
    var dx = b.clientX - a.clientX;
    var dy = b.clientY - a.clientY;
    distance = Math.sqrt(dx * dx + dy * dy);
    mouse.x = dx / 2 + a.clientX;
    mouse.y = dy / 2 + a.clientY;
  }

  function pinchmove(e) 
  {
    for (var i = 0; i < e.touches.length; i++) 
    {
      var touch = e.touches[i];
      touches[i] = touch;
    }
    var a = touches[0];
    var b = touches[1];
    var dx = b.clientX - a.clientX;
    var dy = b.clientY - a.clientY;
    var d = Math.sqrt(dx * dx + dy * dy);
    var delta = d - distance;
    if (delta <= 10 && delta >= -10)
    {
      _internalZoom(delta / 100);
    }
    distance = d;
  }
}