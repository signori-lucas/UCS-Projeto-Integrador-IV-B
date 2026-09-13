document.addEventListener('DOMContentLoaded', () => {
    const selectSex = document.getElementById('filter-sex');
    const selectState = document.getElementById('filter-state');
    const decadeSelector = document.getElementById('decade-selector');
    const rankingList = document.getElementById('ranking-list');
    const btnSearch = document.getElementById('btn-search');
    const btnClear = document.getElementById('btn-clear');
    const inputSearch = document.getElementById('search-name');
    const searchResult = document.getElementById('search-result');
    const resultName = document.getElementById('result-name');
    const resultTotal = document.getElementById('result-total');
    const resultFrequency = document.getElementById('result-frequency');
    function showMessage(text, isError) {
        // messages element removed from layout. Keep console logging for debugging.
        if (isError) console.error(text); else console.log(text);
    }

    async function loadStates() {
        try {
            const res = await fetch('/api/rankings/states');
            const states = await res.json();
            states.forEach(s => {
                const opt = document.createElement('option');
                opt.value = s.code;
                opt.textContent = s.name;
                selectState.appendChild(opt);
            });
        } catch (err) {
            showMessage('Não foi possível carregar as unidades da federação (mock).', true);
        }
    }

    async function loadRanking() {
        const sex = selectSex.value;
        const state = selectState.value;
        rankingList.innerHTML = '<li class="list-group-item">Carregando...</li>';
        try {
            const res = await fetch(`/api/rankings?sex=${encodeURIComponent(sex)}&state=${encodeURIComponent(state)}`);
            const list = await res.json();
            rankingList.innerHTML = '';
            if (!list || list.length === 0) {
                rankingList.innerHTML = '<li class="list-group-item">Nenhum resultado</li>';
                return;
            }
            list.forEach(item => {
                const li = document.createElement('li');
                li.className = 'list-group-item d-flex justify-content-between align-items-center';
                li.innerHTML = `<div><strong>${item.rank}. ${item.name}</strong></div><span class="badge bg-primary rounded-pill">${item.total.toLocaleString()}</span>`;
                rankingList.appendChild(li);
            });
        } catch (err) {
            rankingList.innerHTML = '<li class="list-group-item text-danger">Erro ao carregar ranking</li>';
        }
    }

    async function loadDecadeRanking() {
        const sex = selectSex.value;
        const state = selectState.value;
        const decada = decadeSelector ? decadeSelector.value : '1990';
        const target = document.getElementById('decade-ranking-list');
        target.innerHTML = '<li class="list-group-item">Carregando...</li>';
        try {
            const res = await fetch(`/api/rankings/by-decade?decada=${encodeURIComponent(decada)}&sex=${encodeURIComponent(sex)}&state=${encodeURIComponent(state)}`);
            const list = await res.json();
            target.innerHTML = '';
            if (!list || list.length === 0) {
                target.innerHTML = '<li class="list-group-item">Nenhum resultado</li>';
                return;
            }
            list.forEach(item => {
                const li = document.createElement('li');
                li.className = 'list-group-item d-flex justify-content-between align-items-center';
                li.innerHTML = `<div><strong>${item.rank}. ${item.name}</strong></div><span class="badge bg-secondary rounded-pill">${item.total.toLocaleString()}</span>`;
                target.appendChild(li);
            });
        } catch (err) {
            target.innerHTML = '<li class="list-group-item text-danger">Erro ao carregar ranking por década</li>';
        }
    }

    async function doSearch() {
        const name = inputSearch.value.trim();
        const state = selectState.value;
        if (!name) {
            showMessage('Digite um nome para pesquisar.', true);
            return;
        }
        showMessage('Pesquisando...');
        try {
            const res = await fetch(`/api/names/search?name=${encodeURIComponent(name)}&state=${encodeURIComponent(state)}`);
            if (!res.ok) {
                throw new Error('fail');
            }
            const data = await res.json();
            resultName.textContent = data.name;
            resultTotal.textContent = `Total de registros: ${data.total.toLocaleString()}`;
            resultFrequency.textContent = `Frequência: ${data.frequency}`;
            searchResult.style.display = 'block';
            showMessage('Resultado carregado.');
        } catch (err) {
            showMessage('Erro ao pesquisar nome.', true);
        }
    }

    function clearSearch() {
        // Clear text search
        inputSearch.value = '';
        if (searchResult) searchResult.style.display = 'none';

        // Reset filters to defaults
        if (selectSex) selectSex.value = 'all';
        if (selectState) selectState.value = 'BR';
        if (decadeSelector) {
            // default decade used during init is 1990
            try { decadeSelector.value = '1990'; } catch (e) { decadeSelector.selectedIndex = 0; }
        }

        // Reload rankings with defaults
        loadRanking();
        loadDecadeRanking();

        showMessage('Filtros limpos.');
        inputSearch.focus();
    }

    // Event listeners
    selectSex.addEventListener('change', () => { loadRanking(); loadDecadeRanking(); showMessage('Filtro de sexo aplicado.'); });
    if (decadeSelector) decadeSelector.addEventListener('change', () => { loadDecadeRanking(); showMessage('Década selecionada.'); });
    selectState.addEventListener('change', () => { loadRanking(); loadDecadeRanking(); showMessage('Filtro aplicado.'); });
    btnSearch.addEventListener('click', doSearch);
    if (btnClear) btnClear.addEventListener('click', clearSearch);
    inputSearch.addEventListener('keydown', (e) => { if (e.key === 'Enter') doSearch(); if (e.key === 'Escape') clearSearch(); });

    // Init
    loadStates().then(() => { loadRanking(); loadDecadeRanking(); });
});
