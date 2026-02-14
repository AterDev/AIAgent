// 更贴近真实夜空的星场：色温分布、深度分层、星带聚集与轻微视差
interface Star {
  x: number;
  y: number;
  r: number;
  depth: number; // 0=最远, 1=最近
  baseAlpha: number;
  twinklePhase: number;
  twinkleSpeed: number;
  twinkleAmount: number;
  dx: number;
  dy: number;
  color: string;
  glowColor: string;
}

interface StarColorClass {
  fill: string;
  glow: string;
  weight: number;
}

const REALISTIC_STAR_CLASSES: StarColorClass[] = [
  { fill: 'rgba(167, 195, 255, 1)', glow: 'rgba(167, 195, 255, 0.9)', weight: 0.02 }, // B/O 蓝白
  { fill: 'rgba(205, 222, 255, 1)', glow: 'rgba(205, 222, 255, 0.9)', weight: 0.08 }, // A/F 白
  { fill: 'rgba(239, 243, 255, 1)', glow: 'rgba(239, 243, 255, 0.9)', weight: 0.46 }, // 主体：近白
  { fill: 'rgba(255, 241, 214, 1)', glow: 'rgba(255, 231, 191, 0.85)', weight: 0.28 }, // G/K 黄白
  { fill: 'rgba(255, 218, 176, 1)', glow: 'rgba(255, 205, 153, 0.82)', weight: 0.16 }, // K/M 暖色
];

// 保留外部自定义能力（若调用 setStarColors，将覆盖默认色温池）
export let STAR_COLORS = REALISTIC_STAR_CLASSES.map((x) => x.fill);

export function setStarColors(colors: string[]) {
  STAR_COLORS = colors;
}

function random(min: number, max: number) {
  return Math.random() * (max - min) + min;
}

function gaussian(mean = 0, stdDev = 1) {
  const u = 1 - Math.random();
  const v = Math.random();
  return mean + stdDev * Math.sqrt(-2 * Math.log(u)) * Math.cos(2 * Math.PI * v);
}

function wrap(value: number, max: number) {
  if (value < 0) {
    return value + max;
  }

  if (value > max) {
    return value - max;
  }

  return value;
}

function pickColorClass(): StarColorClass {
  // 若外部覆盖了颜色，使用均匀随机，glow 默认用同色
  if (STAR_COLORS.length > 0 && STAR_COLORS.length !== REALISTIC_STAR_CLASSES.length) {
    const fill = STAR_COLORS[Math.floor(Math.random() * STAR_COLORS.length)];
    return { fill, glow: fill, weight: 1 };
  }

  const r = Math.random();
  let sum = 0;
  for (const item of REALISTIC_STAR_CLASSES) {
    sum += item.weight;
    if (r <= sum) {
      return item;
    }
  }
  return REALISTIC_STAR_CLASSES[REALISTIC_STAR_CLASSES.length - 1];
}

function sampleDepth() {
  const layerPick = Math.random();
  if (layerPick < 0.68) {
    // 远景占多数
    return random(0.05, 0.36);
  }

  if (layerPick < 0.92) {
    return random(0.36, 0.72);
  }

  return random(0.72, 1);
}

function sampleStarPosition(w: number, h: number) {
  // 约 35% 星点聚集成较淡的“星带”，其余均匀分布
  if (Math.random() < 0.35) {
    const x = random(0, w);
    const centerY = h * 0.5 + (x - w * 0.5) * 0.16;
    const y = Math.max(0, Math.min(h, gaussian(centerY, h * 0.16)));
    return { x, y };
  }

  return {
    x: random(0, w),
    y: random(0, h)
  };
}

function createStars(count: number, w: number, h: number): Star[] {
  const stars: Star[] = [];

  for (let i = 0; i < count; i++) {
    const depth = sampleDepth();
    const magnitude = Math.pow(Math.random(), 2.8); // 亮星稀少，暗星常见
    const colorClass = pickColorClass();
    const pos = sampleStarPosition(w, h);

    const r = 0.3 + depth * 1.1 + magnitude * 0.85;
    const baseAlpha = 0.08 + depth * 0.56 + magnitude * 0.2;
    const speed = 0.002 + depth * 0.012;

    stars.push({
      x: pos.x,
      y: pos.y,
      r,
      depth,
      baseAlpha,
      twinklePhase: random(0, Math.PI * 2),
      twinkleSpeed: random(0.35, 1.15),
      twinkleAmount: 0.04 + (1 - depth) * 0.06,
      dx: random(-1, 1) * speed,
      dy: random(-0.8, 0.8) * speed,
      color: colorClass.fill,
      glowColor: colorClass.glow
    });
  }

  return stars;
}

function getStarCount(width: number) {
  return width > 900 ? 54 : 30;
}

export function initStarfield(canvas: HTMLCanvasElement) {
  const context = canvas.getContext('2d');
  if (!context) {
    return;
  }
  const ctx = context;

  let w = 0;
  let h = 0;
  let stars: Star[] = [];
  let pointerX = 0;
  let pointerY = 0;
  let smoothedPointerX = 0;
  let smoothedPointerY = 0;
  let lastTime = performance.now();

  function resize() {
    const dpr = Math.min(window.devicePixelRatio || 1, 2);
    w = canvas.offsetWidth;
    h = canvas.offsetHeight;

    canvas.width = Math.max(1, Math.floor(w * dpr));
    canvas.height = Math.max(1, Math.floor(h * dpr));

    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    stars = createStars(getStarCount(w), w, h);
  }

  function onPointerMove(event: MouseEvent) {
    if (!w || !h) {
      return;
    }

    pointerX = ((event.clientX / w) - 0.5) * 2;
    pointerY = ((event.clientY / h) - 0.5) * 2;
  }

  function draw(now: number) {
    const dt = Math.min((now - lastTime) / 16.67, 2);
    lastTime = now;

    smoothedPointerX += (pointerX - smoothedPointerX) * 0.04;
    smoothedPointerY += (pointerY - smoothedPointerY) * 0.04;

    ctx.clearRect(0, 0, w, h);

    for (const star of stars) {
      star.x = wrap(star.x + star.dx * dt, w);
      star.y = wrap(star.y + star.dy * dt, h);

      const parallax = 0.25 + star.depth * 1.8;
      const sx = wrap(star.x + smoothedPointerX * parallax * 2.8, w);
      const sy = wrap(star.y + smoothedPointerY * parallax * 1.8, h);

      const twinkle = 1 + Math.sin(now * 0.0012 * star.twinkleSpeed + star.twinklePhase) * star.twinkleAmount;
      const depthAttenuation = 0.34 + star.depth * 0.82;
      const alpha = Math.max(0.06, Math.min(1, star.baseAlpha * twinkle * depthAttenuation));

      ctx.save();
      ctx.beginPath();
      ctx.arc(sx, sy, star.r, 0, Math.PI * 2);
      ctx.globalAlpha = alpha;
      ctx.fillStyle = star.color;
      ctx.shadowColor = star.glowColor;
      ctx.shadowBlur = 0.7 + star.depth * 4.2;
      ctx.fill();
      ctx.restore();
    }

    requestAnimationFrame(draw);
  }

  resize();
  window.addEventListener('resize', resize);
  window.addEventListener('mousemove', onPointerMove, { passive: true });
  requestAnimationFrame(draw);
}
