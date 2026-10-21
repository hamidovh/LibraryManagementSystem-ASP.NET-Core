/* ~/wwwroot/AdminControllerJSs/MultiSelect.js: */
/* Bu MultiSelect.js faylının istifadə olunduğu səhifələr: BookIndex.cshtml, BookCreate.cshtml, BookEdit.cshtml, BorrowCreate.cshtml. */
/* Bu MultiSelect.js faylındakı classlara Filter.css faylında CSS verilib. */
/* Rejimlər: <select multiple> - çoxlu seçim; <select data-single> (multiple YOXDUR) - tək seçim (seçəndə dropdown bağlanır, value-su "" və ya "0" olan option placeholder sayılır və siyahıda göstərilmir). */

document.addEventListener(`DOMContentLoaded`, () => {
    const selects = document.querySelectorAll(`.multiselect`);

    selects.forEach(function (select) {
        const wrapper = document.createElement(`div`);
        wrapper.className = `multiselect-wrapper`;

        select.parentNode.insertBefore(wrapper, select);
        wrapper.appendChild(select);

        select.classList.add(`multiselect-source`);

        const button = document.createElement(`button`);
        button.type = `button`;
        button.className = `multiselect-trigger`;

        const selectedText = document.createElement(`span`);
        selectedText.className = `multiselect-value`;

        const arrow = document.createElement(`i`);
        arrow.className = `fa-solid fa-chevron-down multiselect-arrow`;

        button.append(selectedText, arrow);

        const dropdown = document.createElement(`div`);
        dropdown.className = `multiselect-dropdown`;

        const optionsList = document.createElement(`div`);
        optionsList.className = `multiselect-options`;

        const isAutoSubmit = select.hasAttribute(`data-autosubmit`);
        const isSearchable = select.hasAttribute(`data-searchable`); // Axtarış data-searchable atributu ilə işə düşür.
        const isSingle = select.hasAttribute(`data-single`); // Tək seçim rejimi data-single atributu ilə işə düşür.
        const storageKey = `multiselect-open`;

        // Tək seçim rejimində value-su boş və ya 0 olan option placeholder sayılır (məsələn, <option value="0">İstifadəçi seçin...</option>):
        const isEmptyOption = option => isSingle && (option.value === `` || option.value === `0`);

        let searchTerm = ``;
        let searchInput = null;

        // Axtarış sahəsi (yalnız data-searchable olan selectlər üçün):
        if (isSearchable) {
            const searchBox = document.createElement(`div`);
            searchBox.className = `multiselect-search`;

            const searchIcon = document.createElement(`i`);
            searchIcon.className = `fa-solid fa-magnifying-glass`;

            searchInput = document.createElement(`input`);
            searchInput.type = `text`;
            searchInput.className = `multiselect-search-input`;
            searchInput.placeholder = `Axtar...`;
            searchInput.autocomplete = `off`;

            // Yazdıqca axtarış mətni (kiçik hərflə, Azərbaycan əlifbası ilə) saxlanılır və siyahı yenilənir:
            searchInput.addEventListener(`input`, function () {
                searchTerm = searchInput.value.trim().toLocaleLowerCase(`az`);
                renderOptions();
            });

            // Enter basanda form göndərilmir:
            searchInput.addEventListener(`keydown`, function (event) {
                if (event.key === `Enter`) {
                    event.preventDefault();
                }
            }); // Enter: axtarış sahəsində Enter basanda form göndərilmir.

            searchBox.append(searchIcon, searchInput);
            dropdown.appendChild(searchBox);
        }

        dropdown.appendChild(optionsList);
        wrapper.append(button, dropdown);

        function updateSelectedText() {
            const selectedOptions = Array.from(select.selectedOptions).filter(option => !isEmptyOption(option));
            if (selectedOptions.length === 0) {
                selectedText.textContent = select.dataset.placeholder || `Seçim edin...`;
                selectedText.classList.add(`multiselect-placeholder`);
            }
            else {
                selectedText.textContent = selectedOptions.map(option => option.textContent.trim()).join(`, `);
                selectedText.classList.remove(`multiselect-placeholder`);
            }
        }

        function renderOptions() {
            optionsList.replaceChildren();

            let visibleCount = 0;

            Array.from(select.options).forEach(function (option) {
                // Tək seçim rejimində placeholder option siyahıda göstərilmir:
                if (isEmptyOption(option)) {
                    return;
                }

                const optionText = option.textContent.trim();

                // Axtarışa uyğun gəlməyən elementlər göstərilmir:
                if (searchTerm !== `` && !optionText.toLocaleLowerCase(`az`).includes(searchTerm)) {
                    return;
                }

                visibleCount++;

                const item = document.createElement(`button`);
                item.type = `button`;
                item.className = `multiselect-option`;

                const check = document.createElement(`span`);
                check.className = `multiselect-check`;

                const name = document.createElement(`span`);
                name.textContent = optionText;

                if (option.selected) {
                    item.classList.add(`is-selected`);
                    check.textContent = `✓`;
                }

                item.append(check, name);
                optionsList.appendChild(item);

                item.addEventListener(`click`, function () {
                    if (isSingle) {
                        // Tək seçim: əvvəlki seçim silinir, kliklənən seçilir (artıq seçilmişə yenidən klik seçimi təmizləyir):
                        const wasSelected = option.selected;

                        Array.from(select.options).forEach(function (other) {
                            other.selected = false;
                        });

                        if (wasSelected) {
                            const emptyOption = Array.from(select.options).find(isEmptyOption);
                            if (emptyOption) {
                                emptyOption.selected = true;
                            }
                        }
                        else {
                            option.selected = true;
                        }

                        updateSelectedText();
                        closeDropdown();
                        renderOptions();

                        // Seçimdən sonra xəta mesajı (varsa) yenidən yoxlanılaraq yox olur (gizli select-ə birbaşa klik olmadığı üçün jQuery Validation özü yoxlamır):
                        try {
                            if (window.jQuery && window.jQuery.fn.valid) {
                                window.jQuery(select).valid();
                            }
                        } catch (e) { }
                    }
                    else {
                        option.selected = !option.selected;

                        updateSelectedText();
                        renderOptions();
                    }

                    // Filtr multiselectlərində klik edən kimi form göndərilir:
                    if (isAutoSubmit && select.form) {
                        try {
                            sessionStorage.setItem(storageKey, select.name);
                        } catch (e) { }

                        select.form.submit();
                    }
                });
            });

            // Heç nə tapılmadıqda mesaj göstərilir:
            if (visibleCount === 0) {
                const empty = document.createElement(`div`);
                empty.className = `multiselect-empty`;
                empty.textContent = `Nəticə tapılmadı`;
                optionsList.appendChild(empty);
            } // Tapılmadı: heç nə uyğun gəlməsə "Nəticə tapılmadı" yazısı çıxır.
        }

        function openDropdown() {
            // Digər açıq dropdownlar bağlanır:
            document.querySelectorAll(`.multiselect-wrapper.is-open`).forEach(function (other) {
                if (other !== wrapper) {
                    other.dispatchEvent(new CustomEvent(`multiselect:close`));
                }
            });

            wrapper.classList.add(`is-open`);
            renderOptions();

            // Dropdown açılan kimi yazmağa başlamaq üçün axtarış sahəsi fokuslanır:
            if (searchInput) {
                searchInput.focus();
            } // Açılış: dropdown açılan kimi cursor axtarış sahəsində olur, əlavə klik lazım deyil.
        }

        function closeDropdown() {
            wrapper.classList.remove(`is-open`);

            // Axtarış sıfırlanır ki, növbəti açılışda bütün siyahı görünsün:
            if (searchInput && searchTerm !== ``) {
                searchInput.value = ``;
                searchTerm = ``;
                renderOptions();
            } // Bağlanma: dropdown bağlananda axtarış təmizlənir, növbəti açılışda bütün siyahı görünür.
        }

        button.addEventListener(`click`, function () {
            if (wrapper.classList.contains(`is-open`)) {
                closeDropdown();
            }
            else {
                openDropdown();
            }
        });

        wrapper.addEventListener(`multiselect:close`, closeDropdown);

        // Kənara klikləyəndə bağlanır (composedPath istifadə olunur, çünki option-a klik edəndə renderOptions() həmin elementi DOM-dan silir və wrapper.contains() səhv nəticə verər).
        document.addEventListener(`click`, function (event) {
            if (!event.composedPath().includes(wrapper)) {
                closeDropdown();
            }
        });

        document.addEventListener(`keydown`, function (event) {
            if (event.key === `Escape`) {
                closeDropdown();
            }
        });

        updateSelectedText();
        renderOptions();

        // Səhifə filtrasiyadan sonra yenilənəndə dropdown açıq qalır:
        if (isAutoSubmit) {
            try {
                if (sessionStorage.getItem(storageKey) === select.name) {
                    wrapper.classList.add(`is-open`);
                    sessionStorage.removeItem(storageKey);
                }
            } catch (e) { }
        }
    });
});
