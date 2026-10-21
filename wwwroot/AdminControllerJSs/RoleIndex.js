/* ~/wwwroot/AdminControllerJSs/RoleIndex.js: */

document.addEventListener(`DOMContentLoaded`, () => {
    // ROLE-MODAL ELEMENTS START
    const roleModal = document.getElementById(`roleModal`);
    const modalContainer = document.getElementById(`modalContainer`);
    const popupClose = document.getElementById(`popupClose`);
    // ROLE-MODAL ELEMENTS END

    // POPUP ELEMENTS START
    const popupRoleId = document.getElementById(`popupRoleId`);
    const popupRoleTitle = document.getElementById(`popupRoleTitle`);
    const popupRoleName = document.getElementById(`popupRoleName`);
    const popupRoleUserCount = document.getElementById(`popupRoleUserCount`);
    const popupRoleIsActive = document.getElementById(`popupRoleIsActive`);
    const popupRoleCreatedDate = document.getElementById(`popupRoleCreatedDate`);
    const popupRoleUpdatedDate = document.getElementById(`popupRoleUpdatedDate`);
    // POPUP ELEMENTS END

    // DETAILSBUTTON START
    const detailsButton = document.querySelectorAll(`.btn-details`);

    detailsButton.forEach((button) => {
        button.addEventListener(`click`, () => {
            // BUTTON DATA START
            const roleId = button.dataset.roleId;
            const roleName = button.dataset.roleName;
            const roleUserCount = button.dataset.roleUserCount;
            const roleIsActive = button.dataset.roleIsActive;
            const roleCreatedDate = button.dataset.roleCreatedDate;
            const roleUpdatedDate = button.dataset.roleUpdatedDate;
            // BUTTON DATA END

            // POPUP TEXT DATA START
            // RoleID:
            popupRoleId.textContent = roleId || `-`;
            // RoleTitle and RoleName:
            popupRoleTitle.textContent = roleName || `Rol`;
            popupRoleName.textContent = roleName || `-`;
            // RoleUserCount:
            popupRoleUserCount.textContent = roleUserCount || `0`;
            // RoleIsActive:
            popupRoleIsActive.classList.remove(`active`, `deactive`);
            if (roleIsActive === `True` || roleIsActive === `true`) {
                popupRoleIsActive.textContent = `Aktiv`;
                popupRoleIsActive.classList.add(`active`);
            }
            else {
                popupRoleIsActive.textContent = `Deaktiv`;
                popupRoleIsActive.classList.add(`deactive`);
            }
            // RoleCreatedDate:
            popupRoleCreatedDate.textContent = roleCreatedDate || `-`;
            // RoleUpdatedDate:
            popupRoleUpdatedDate.textContent = roleUpdatedDate || `-`;
            // POPUP TEXT DATA END

            // Popupı açır:
            roleModal.classList.add(`active`);

            // Popup açıq olanda bodynin scroll edilə bilinməsini bağlayır:
            document.body.style.overflow = `hidden`;
        });
    });
    // DETAILSBUTTON END

    function closeModal() {
        roleModal.classList.remove(`active`);

        // Popup bağlandıqda bodynin scroll edilə bilinməsini aktivləşdirir:
        document.body.style.overflow = ``;
    }

    popupClose.addEventListener(`click`, closeModal);

    modalContainer.addEventListener(`click`, closeModal);

    // Esc düyməsi ilə popupı bağlamaq:
    document.addEventListener(`keydown`, (event) => {
        if (event.key === `Escape` && roleModal.classList.contains(`active`)) {
            closeModal();
        }
    });
});
