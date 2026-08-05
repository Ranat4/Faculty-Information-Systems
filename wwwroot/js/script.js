
document.querySelectorAll('.tab-btn').forEach(btn => {
  btn.addEventListener('click', () => {
    document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
    document.querySelectorAll('.tab-panel').forEach(p => p.classList.remove('active'));
    btn.classList.add('active');
    document.getElementById(btn.dataset.tab).classList.add('active');
  });
});

const menuToggle = document.querySelector('.menu-toggle');
const navPill = document.querySelector('.nav-pill');

if (menuToggle && navPill) {
  menuToggle.addEventListener('click', () => {
    const isOpen = navPill.style.display === 'flex';
    navPill.style.display = isOpen ? 'none' : 'flex';
    navPill.style.flexDirection = 'column';
    navPill.style.position = 'absolute';
    navPill.style.top = '78px';
    navPill.style.left = '20px';
    navPill.style.right = '20px';
    navPill.style.borderRadius = '20px';
    navPill.style.zIndex = '40';
  });
}

const dots = document.querySelectorAll('.dots span');
if (dots.length) {
  let current = 0;
  setInterval(() => {
    dots[current].classList.remove('active');
    current = (current + 1) % dots.length;
    dots[current].classList.add('active');
  }, 3500);
}
