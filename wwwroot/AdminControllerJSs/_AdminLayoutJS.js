/* ~/wwwroot/AdminControllerJSs/_AdminLayoutJS.js: */

const showhideBtn = document.getElementById(`showhide`);
const sidebar = document.querySelector(`.sidebar-left`);

showhideBtn.addEventListener(`click`, () => {
    sidebar.classList.toggle(`sidebar-opened`);
});
