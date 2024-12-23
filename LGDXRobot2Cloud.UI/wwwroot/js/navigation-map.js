var canvas;
var two;
var windowHeight = window.innerHeight;
var zui;
var startScale;

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

  AddZui(canvas.width, canvas.height, imageWidth, imageHeight);

  // Disabe image smoothing
  canvas.getContext("2d").imageSmoothingEnabled = false;
}

function NavigationMapZoomIn()
{
  zui.zoomBy(0.2, canvas.width / 2, canvas.height / 2);
}

function NavigationMapZoomOut()
{
  zui.zoomBy(-0.2, canvas.width / 2, canvas.height / 2);
}

function NavigationMapZoomReset()
{
  zui.zoomSet(startScale, canvas.width / 2, canvas.height / 2);
}

window.addEventListener("resize", function(e)
{
  e.preventDefault;

  // Set Canvas size
  let newWindowHeight = window.innerHeight;
  let diff = newWindowHeight - windowHeight;
  let div = document.getElementById("navigation-map-div");
  two.renderer.setSize(div.clientWidth, div.clientHeight + (diff < 0 ? diff : 0));

  // Set ZUI limits
  let mapImage = document.getElementById("navigation-map-image");
  SetScale(div.clientWidth, div.clientHeight, mapImage.width, mapImage.height);

  windowHeight = newWindowHeight;

  // Disabe image smoothing
  canvas.getContext("2d").imageSmoothingEnabled = false;
});

function SetScale(canvasWidth, canvasHeight, imageWidth, imageHeight)
{
  // Fit map to screen
  const canvasAspectRatio = canvasWidth / canvasHeight;
  const imageAspectRatio = imageWidth / imageHeight;
  if (imageAspectRatio > canvasAspectRatio) 
  {
    // Image is wider than canvas
    startScale = canvasWidth / imageWidth;
  }
  else 
  {
    // Image is taller than canvas
    startScale = canvasHeight / imageHeight;
  }
  zui.limits.scale.min = startScale;
  zui.limits.scale.max = 10;
}

function AddZui(canvasWidth, canvasHeight, imageWidth, imageHeight)
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
  let dragging = false;

  SetScale(canvasWidth, canvasHeight, imageWidth, imageHeight);
  zui.zoomSet(startScale, canvasWidth / 2, canvasHeight / 2);

  domElement.addEventListener('mousedown', mousedown, false);
  domElement.addEventListener('mousewheel', mousewheel, false);
  domElement.addEventListener('wheel', mousewheel, false);

  domElement.addEventListener('touchstart', touchstart, false);
  domElement.addEventListener('touchmove', touchmove, false);
  domElement.addEventListener('touchend', touchend, false);
  domElement.addEventListener('touchcancel', touchend, false);

  function mousedown(e) {
    mouse.x = e.clientX;
    mouse.y = e.clientY;
    var rect = scene.getBoundingClientRect();
    dragging = mouse.x > rect.left && mouse.x < rect.right
      && mouse.y > rect.top && mouse.y < rect.bottom;
    window.addEventListener('mousemove', mousemove, false);
    window.addEventListener('mouseup', mouseup, false);
  }

  function mousemove(e) {
    var dx = e.clientX - mouse.x;
    var dy = e.clientY - mouse.y;
    if (dragging) {
      scene.position.x += dx;
      scene.position.y += dy;
    } else {
      zui.translateSurface(dx, dy);
    }
    mouse.set(e.clientX, e.clientY);
  }

  function mouseup(e) {
    window.removeEventListener('mousemove', mousemove, false);
    window.removeEventListener('mouseup', mouseup, false);
  }

  function mousewheel(e) {
    var dy = (e.wheelDeltaY || - e.deltaY) / 1000;
    zui.zoomBy(dy, e.clientX, e.clientY);
  }

  function touchstart(e) {
    switch (e.touches.length) {
      case 2:
        pinchstart(e);
        break;
      case 1:
        panstart(e)
        break;
    }
  }

  function touchmove(e) {
    switch (e.touches.length) {
      case 2:
        pinchmove(e);
        break;
      case 1:
        panmove(e)
        break;
    }
  }

  function touchend(e) {
    touches = {};
    var touch = e.touches[ 0 ];
    if (touch) {  // Pass through for panning after pinching
      mouse.x = touch.clientX;
      mouse.y = touch.clientY;
    }
  }

  function panstart(e) {
    var touch = e.touches[ 0 ];
    mouse.x = touch.clientX;
    mouse.y = touch.clientY;
  }

  function panmove(e) {
    var touch = e.touches[ 0 ];
    var dx = touch.clientX - mouse.x;
    var dy = touch.clientY - mouse.y;
    zui.translateSurface(dx, dy);
    mouse.set(touch.clientX, touch.clientY);
  }

  function pinchstart(e) {
    for (var i = 0; i < e.touches.length; i++) {
      var touch = e.touches[ i ];
      touches[ touch.identifier ] = touch;
    }
    var a = touches[ 0 ];
    var b = touches[ 1 ];
    var dx = b.clientX - a.clientX;
    var dy = b.clientY - a.clientY;
    distance = Math.sqrt(dx * dx + dy * dy);
    mouse.x = dx / 2 + a.clientX;
    mouse.y = dy / 2 + a.clientY;
  }

  function pinchmove(e) {
    for (var i = 0; i < e.touches.length; i++) {
      var touch = e.touches[ i ];
      touches[ touch.identifier ] = touch;
    }
    var a = touches[ 0 ];
    var b = touches[ 1 ];
    var dx = b.clientX - a.clientX;
    var dy = b.clientY - a.clientY;
    var d = Math.sqrt(dx * dx + dy * dy);
    var delta = d - distance;
    zui.zoomBy(delta / 250, mouse.x, mouse.y);
    distance = d;
  }
}