// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Funcionalidad del Sidebar
$(document).ready(function () {
    const sidebar = $('#sidebar');
    const sidebarOverlay = $('#sidebarOverlay');
    const sidebarCollapse = $('#sidebarCollapse');
    
    function isMobile() {
        return window.innerWidth <= 768;
    }
    
    // Función para abrir sidebar
    function openSidebar() {
        sidebar.addClass('active');
        if (isMobile()) {
            sidebarOverlay.addClass('active');
            $('body').css('overflow', 'hidden');
        }
    }
    
    // Función para cerrar sidebar
    function closeSidebar() {
        sidebar.removeClass('active');
        sidebarOverlay.removeClass('active');
        $('body').css('overflow', '');
    }
    
    // Toggle sidebar con botón
    sidebarCollapse.on('click', function (e) {
        e.preventDefault();
        e.stopPropagation();
        
        if (sidebar.hasClass('active')) {
            closeSidebar();
        } else {
            openSidebar();
        }
    });
    
    // Cerrar sidebar al hacer click en el overlay
    sidebarOverlay.on('click', function (e) {
        e.stopPropagation();
        closeSidebar();
    });
    
    // Prevenir que el click en el sidebar cierre el sidebar
    sidebar.on('click', function (e) {
        e.stopPropagation();
    });
    
    // Funcionalidad de arrastre (swipe) para cerrar sidebar
    let swipeStartX = 0;
    let swipeStartY = 0;
    let isHorizontalSwipe = false;
    
    // Detectar swipe en el sidebar
    sidebar.on('touchstart', function (e) {
        if (isMobile() && sidebar.hasClass('active')) {
            swipeStartX = e.originalEvent.touches[0].clientX;
            swipeStartY = e.originalEvent.touches[0].clientY;
            isHorizontalSwipe = false;
        }
    });
    
    sidebar.on('touchmove', function (e) {
        if (!isMobile() || !sidebar.hasClass('active')) return;
        
        const currentX = e.originalEvent.touches[0].clientX;
        const currentY = e.originalEvent.touches[0].clientY;
        const deltaX = Math.abs(swipeStartX - currentX);
        const deltaY = Math.abs(swipeStartY - currentY);
        
        // Determinar si es un swipe horizontal
        if (deltaX > 10 && deltaX > deltaY) {
            isHorizontalSwipe = true;
            // Solo prevenir scroll si es un swipe horizontal significativo hacia la izquierda
            if (swipeStartX - currentX > 20) {
                e.preventDefault();
            }
        }
    });
    
    sidebar.on('touchend', function (e) {
        if (!isMobile() || !sidebar.hasClass('active')) {
            isHorizontalSwipe = false;
            return;
        }
        
        const touchEndX = e.originalEvent.changedTouches[0].clientX;
        const touchEndY = e.originalEvent.changedTouches[0].clientY;
        const deltaX = swipeStartX - touchEndX;
        const deltaY = Math.abs(swipeStartY - touchEndY);
        
        // Cerrar si es un swipe horizontal hacia la izquierda (>50px)
        if (isHorizontalSwipe && deltaX > 50 && deltaY < 100) {
            closeSidebar();
        }
        
        isHorizontalSwipe = false;
    });
    
    // También detectar swipe en el overlay
    sidebarOverlay.on('touchstart', function (e) {
        if (isMobile() && sidebar.hasClass('active')) {
            swipeStartX = e.originalEvent.touches[0].clientX;
            swipeStartY = e.originalEvent.touches[0].clientY;
        }
    });
    
    sidebarOverlay.on('touchend', function (e) {
        if (!isMobile() || !sidebar.hasClass('active')) return;
        
        const touchEndX = e.originalEvent.changedTouches[0].clientX;
        const touchEndY = e.originalEvent.changedTouches[0].clientY;
        const deltaX = swipeStartX - touchEndX;
        const deltaY = Math.abs(swipeStartY - touchEndY);
        
        // Cerrar con swipe izquierda en el overlay
        if (deltaX > 50 && deltaY < 100) {
            closeSidebar();
        }
    });
    
    // Cerrar sidebar cuando cambia el tamaño de la ventana a desktop
    $(window).on('resize', function () {
        if (!isMobile()) {
            closeSidebar();
        }
    });
});

// Las alertas permanecen visibles hasta que el usuario las cierre manualmente
// Se eliminó el auto-ocultamiento automático para que las alertas importantes (como las de costos y gastos) permanezcan visibles
