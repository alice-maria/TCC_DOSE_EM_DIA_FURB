<template>
  <div class="acessibilidade-container" :class="{ 'menu-aberto': menuAberto }">
    <div class="acessibilidade-wrap">
      <button class="acessibilidade-botao" @click="onAcessClick">
        <img :src="iconeVlibra" alt="Acessibilidade" class="icone" />
      </button>

      <div class="acessibilidade-tooltip" role="tooltip">
        <div class="tooltip-body">
          Ative recursos de acessibilidade: contraste alto, aumento de fonte,
          fonte para dislexia, e ajustes de espaçamento.
        </div>
      </div>
    </div>

    <div v-if="menuAberto" class="acessibilidade-menu">
      <button @click="toggleContraste">
        <img src="@/assets/icons/acessibilidade/contrasteAlto.svg" alt="Contraste" class="icone-menu" />
        Contraste alto
      </button>
      <button @click="aumentarFonte">
        <img src="@/assets/icons/acessibilidade/aumentarFonte.svg" alt="Fonte" class="icone-menu" />
        Aumentar fonte
      </button>
      <button @click="toggleFonteDislexia">
        <img src="@/assets/icons/acessibilidade/dislexia.svg" alt="Dislexia" class="icone-menu" />
        Dislexia
      </button>
      <button @click="toggleEspacamentoLetras">
        <img src="@/assets/icons/acessibilidade/espacamentoLinha.svg" alt="Letras" class="icone-menu" />
        Espaçamento entre letras
      </button>
      <button @click="toggleEspacamentoLinhas">
        <img src="@/assets/icons/acessibilidade/espacamentoLetras.svg" alt="Linhas" class="icone-menu" />
        Espaçamento entre linhas
      </button>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      menuAberto: false,
      fonteNivel: 0,
      espacamentoNivel: 0,
      linhaNivel: 0,
      iconeVlibra: require("@/assets/icons/contrasteTela.png"),
    };
  },
  methods: {
    toggleMenu() {
      this.menuAberto = !this.menuAberto;
    },
    onAcessClick(e) {
      this.toggleMenu();
      e.currentTarget.blur(); 
    },
    toggleContraste() {
      document.body.classList.toggle("contraste-alto");
    },
    aumentarFonte() {
      this.fonteNivel = (this.fonteNivel + 1) % 4;
      document.body.classList.remove("fonte-baixa", "fonte-media", "fonte-alta");

      if (this.fonteNivel === 1) document.body.classList.add("fonte-baixa");
      else if (this.fonteNivel === 2) document.body.classList.add("fonte-media");
      else if (this.fonteNivel === 3) document.body.classList.add("fonte-alta");
    },
    toggleFonteDislexia() {
      document.body.classList.toggle("fonte-dislexia");
    },
    toggleEspacamentoLetras() {
      this.espacamentoNivel = (this.espacamentoNivel + 1) % 4;
      document.body.classList.remove(
        "espacamento-letras-baixo",
        "espacamento-letras-medio",
        "espacamento-letras-alto"
      );

      if (this.espacamentoNivel === 1) document.body.classList.add("espacamento-letras-baixo");
      else if (this.espacamentoNivel === 2) document.body.classList.add("espacamento-letras-medio");
      else if (this.espacamentoNivel === 3) document.body.classList.add("espacamento-letras-alto");
    },
    toggleEspacamentoLinhas() {
      this.linhaNivel = (this.linhaNivel + 1) % 4;
      document.body.classList.remove(
        "espacamento-linhas-baixo",
        "espacamento-linhas-medio",
        "espacamento-linhas-alto"
      );

      if (this.linhaNivel === 1) document.body.classList.add("espacamento-linhas-baixo");
      else if (this.linhaNivel === 2) document.body.classList.add("espacamento-linhas-medio");
      else if (this.linhaNivel === 3) document.body.classList.add("espacamento-linhas-alto");
    }
  },
};
</script>

<style scoped>
:root {
  --acc-bg: #ffffff;
  --acc-surface: #f5f6f8;
  --acc-border: #e5e7eb;
  --acc-text: #111827;
  --acc-primary: #3084ee;
  --acc-shadow: 0 4px 16px rgba(17, 24, 39, 0.12), 0 1px 2px rgba(0, 0, 0, 0.05);
  --acc-shadow-sm: 0 2px 8px rgba(17, 24, 39, 0.10);
}

.acessibilidade-container {
  position: fixed;
  right: 12px;
  bottom: 405px;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  z-index: 9999;
}

.acessibilidade-botao {
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 9px;
  background: linear-gradient(180deg, #3b8df0, #2a79e0);
  border: none;
  cursor: pointer;
  box-shadow: var(--acc-shadow);
  position: relative;
  z-index: 10002;
}

.icone {
  width: 28px !important;
  height: 28px !important;
  object-fit: contain;
  filter: drop-shadow(0 1px 1px rgba(0, 0, 0, .2));
  pointer-events: none;
}

.acessibilidade-menu {
  position: absolute;
  right: 56px;
  top: 0;
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding: 14px;
  min-width: 420px;
  background: #fff;
  border: 1px solid var(--acc-border);
  border-radius: 14px;
  box-shadow: var(--acc-shadow);
  z-index: 10000;
}

.acessibilidade-menu button {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  padding: 14px 12px;
  font-size: 18px;
  color: var(--acc-text);
  background: var(--acc-surface);
  border: 1px solid var(--acc-border);
  border-radius: 12px;
  cursor: pointer;
  box-shadow: var(--acc-shadow-sm);
  transition: transform .12s ease, box-shadow .12s ease, background-color .12s ease;
  text-align: left;
}

.acessibilidade-menu button:hover {
  transform: translateY(-1px);
  box-shadow: 0 6px 18px rgba(17, 24, 39, .12);
  background: #eef2f7;
}

.acessibilidade-menu button:active {
  transform: translateY(0);
  box-shadow: var(--acc-shadow-sm);
}

.icone-menu {
  width: 28px;
  height: 28px;
  flex: 0 0 28px;
  margin-right: 2px;
}

.acessibilidade-wrap {
  position: relative;
  display: inline-block;
}

.acessibilidade-tooltip {
  position: absolute;
  right: 56px;
  top: -4px;
  max-width: 420px;
  min-width: 360px;
  padding: 10px 16px;
  background: linear-gradient(180deg, #3b8df0, #2a79e0);
  border-radius: 10px;
  box-shadow: 0 6px 16px rgba(17,24,39,.14), 0 2px 6px rgba(0,0,0,.06);
  opacity: 0;
  transform: translateY(6px) scale(0.98);
  pointer-events: none;
  transition: opacity .18s ease, transform .18s ease;
  z-index: 10001;
}

.acessibilidade-wrap:hover .acessibilidade-tooltip,
.acessibilidade-wrap:focus-within .acessibilidade-tooltip {
  opacity: 1;
  transform: translateY(0) scale(1);
}

.menu-aberto .acessibilidade-tooltip {
  opacity: 0 !important;
  transform: translateY(6px) scale(0.98) !important;
  pointer-events: none !important;
}

.tooltip-body {
  font-size: 13px;
  line-height: 1.2;
  color: #fff;
}

@media (max-width: 600px) {
  .acessibilidade-container {
    right: 12px;
    /* Altura do VLibras (~56px) + espaçamento (~24px) */
    bottom: calc(env(safe-area-inset-bottom) + 56px + 24px);
    z-index: 10000; /* garante que fique clicável */
  }

  .acessibilidade-wrap .acessibilidade-tooltip {
    display: none;
  }

  .acessibilidade-menu {
    top: auto;
    bottom: 56px;
    right: 0;
    min-width: 0;
    width: min(92vw, 380px);
    padding: 10px;
    gap: 8px;
    border-radius: 12px;
    box-shadow: var(--acc-shadow-sm);
  }

  .acessibilidade-menu button {
    padding: 12px 10px;
    font-size: 16px;      
    border-radius: 10px;
  }

  .icone-menu {
    width: 24px;
    height: 24px;
    flex: 0 0 24px;
    margin-right: 2px;
  }
}

@media (max-width: 360px) {
  .acessibilidade-menu {
    width: 94vw;
  }
}
</style>