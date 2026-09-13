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
        if (isError) console.error(text); else console.log(text);
    }

    async function loadStates() {
        try {
            const res = await fetch('/api/rankings/states');
            const states = await res.json();
            states.forEach(s => {
                const opt = document.createElement('option');
                opt.value = s.id;
                opt.textContent = s.name;
                selectState.appendChild(opt);
            });
        } catch (err) {
            showMessage('Não foi possível carregar as unidades da federação.', true);
        }
    }

    async function loadRanking() {
        const sex = selectSex.value;
        const state = selectState.value;
        rankingList.innerHTML = '<li class="list-group-item">Carregando...</li>';
        try {
            let url = `/api/rankings?sex=${encodeURIComponent(sex)}`;
            if (state) url += `&stateId=${encodeURIComponent(state)}`;
            const res = await fetch(url);
            const data = await res.json();
            rankingList.innerHTML = '';
            // Data may be returned in different shapes; normalize below
            let items = [];
            if (Array.isArray(data) && data.length > 0) {
                const first = data[0];
                if (first && first.res && Array.isArray(first.res)) {
                    items = first.res.map((r, i) => ({ rank: r.ranking ?? (i + 1), name: r.nome, total: r.frequencia }));
                } else if (first && (first.nome || first.frequencia)) {
                    items = data.map((r, i) => ({ rank: r.ranking ?? (i + 1), name: r.nome, total: r.frequencia }));
                } else {
                    // already in NameRanking shape
                    items = data.map((r, i) => ({ rank: r.rank ?? (i + 1), name: r.name ?? r.nome ?? '', total: r.total ?? r.frequencia ?? 0 }));
                }
            }

            if (!items || items.length === 0) {
                rankingList.innerHTML = '<li class="list-group-item">Nenhum resultado</li>';
                return;
            }

            items.forEach(item => {
                const li = document.createElement('li');
                li.className = 'list-group-item d-flex justify-content-between align-items-center';
                li.innerHTML = `<div><strong>${item.rank}. ${item.name}</strong></div><span class="badge bg-primary rounded-pill">${Number(item.total).toLocaleString()}</span>`;
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
            let url = `/api/rankings/by-decade?decada=${encodeURIComponent(decada)}&sex=${encodeURIComponent(sex)}`;
            if (state) url += `&stateId=${encodeURIComponent(state)}`;
            const res = await fetch(url);
            const data = await res.json();
            target.innerHTML = '';
            let items = [];
            if (Array.isArray(data) && data.length > 0) {
                const first = data[0];
                if (first && first.res && Array.isArray(first.res)) {
                    items = first.res.map((r, i) => ({ rank: r.ranking ?? (i + 1), name: r.nome, total: r.frequencia }));
                } else if (first && (first.nome || first.frequencia)) {
                    items = data.map((r, i) => ({ rank: r.ranking ?? (i + 1), name: r.nome, total: r.frequencia }));
                } else {
                    items = data.map((r, i) => ({ rank: r.rank ?? (i + 1), name: r.name ?? r.nome ?? '', total: r.total ?? r.frequencia ?? 0 }));
                }
            }

            if (!items || items.length === 0) {
                target.innerHTML = '<li class="list-group-item">Nenhum resultado</li>';
                return;
            }

            items.forEach(item => {
                const li = document.createElement('li');
                li.className = 'list-group-item d-flex justify-content-between align-items-center';
                li.innerHTML = `<div><strong>${item.rank}. ${item.name}</strong></div><span class="badge bg-secondary rounded-pill">${Number(item.total).toLocaleString()}</span>`;
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
            const decada = decadeSelector ? decadeSelector.value : '';
            let url = `/api/names/search?name=${encodeURIComponent(name)}`;
            if (decada) url += `&decada=${encodeURIComponent(decada)}`;
            if (state) url += `&stateId=${encodeURIComponent(state)}`;
            const res = await fetch(url);
            if (res.status === 404) {
                // Name not found
                if (searchResult) searchResult.style.display = 'block';
                if (resultName) resultName.textContent = '';
                if (resultTotal) resultTotal.textContent = '';
                if (resultFrequency) resultFrequency.textContent = '';
                const msg = document.getElementById('search-message');
                if (msg) {
                    msg.textContent = 'Nome não encontrado';
                    msg.style.display = 'block';
                }
                return;
            }

            if (!res.ok) {
                throw new Error('fail');
            }

            const data = await res.json();
            // Hide previous message
            const msg = document.getElementById('search-message');
            if (msg) { msg.textContent = ''; msg.style.display = 'none'; }

            resultName.textContent = data.name;
            resultTotal.textContent = `Total de registros: ${data.total.toLocaleString()}`;
            resultFrequency.textContent = `Frequência: ${data.frequency}`;
            searchResult.style.display = 'block';
            showMessage('Resultado carregado.');
        } catch (err) {
            showMessage('Erro ao pesquisar nome.', true);
            // show generic error in UI
            if (searchResult) searchResult.style.display = 'block';
            const msg = document.getElementById('search-message');
            if (msg) { msg.textContent = 'Erro ao pesquisar nome.'; msg.style.display = 'block'; }
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
