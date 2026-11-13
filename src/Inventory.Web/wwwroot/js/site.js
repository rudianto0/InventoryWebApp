document.addEventListener('DOMContentLoaded', () => {
  const body = document.body;
  const toggle = document.getElementById('sidebarToggle');
  const backdrop = document.getElementById('sidebarBackdrop');
  const sidebar = document.getElementById('appSidebar');

  const open = () => {
    body.classList.add('sidebar-open');
    toggle?.setAttribute('aria-expanded', 'true');
  };

  const close = () => {
    body.classList.remove('sidebar-open');
    toggle?.setAttribute('aria-expanded', 'false');
  };

  toggle?.addEventListener('click', () => {
    if (body.classList.contains('sidebar-open')) {
      close();
    } else {
      open();
    }
  });

  backdrop?.addEventListener('click', close);

  document.addEventListener('keydown', (event) => {
    if (event.key === 'Escape') {
      close();
    }
  });

  sidebar?.querySelectorAll('.nav-link').forEach((link) => {
    link.addEventListener('click', () => {
      if (window.innerWidth < 992) {
        close();
      }
    });
  });
});
