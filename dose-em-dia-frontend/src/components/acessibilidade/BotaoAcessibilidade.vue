<template>
  <div class="acessibilidade-container">
    <!-- Botão de acessibilidade -->
    <button class="acessibilidade-botao" @click="toggleMenu">
      <img :src="iconeVlibra" alt="Acessibilidade" class="icone" />
    </button>

    <!-- Menu de acessibilidade -->
    <div v-if="menuAberto" class="acessibilidade-menu">
      <button @click="toggleContraste">
        <img src="@/assets/icons/acessibilidade/contrasteAlto.svg" alt="Contraste" class="icone-contraste" />
        Contraste alto</button>
      <button @click="aumentarFonte">
        <img src="@/assets/icons/acessibilidade/aumentarFonte.svg" alt="Contraste" class="icone-fonte" />
        Aumentar fonte</button>
      <button @click="toggleFonteDislexia">
        <img src="@/assets/icons/acessibilidade/dislexia.svg" alt="Contraste" class="icone-dislexia" />
        Dislexia</button>
      <button @click="toggleEspacamentoLetras">
        <img src="@/assets/icons/acessibilidade/espacamentoLinha.svg" alt="Contraste" class="icone-letras" />
        Espaçamento entre letras</button>
      <button @click="toggleEspacamentoLinhas">
        <img src="@/assets/icons/acessibilidade/espacamentoLetras.svg" alt="Contraste" class="icone-linhas" />
        Espaçamento entre linhas</button>
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
      document.body.classList.remove("espacamento-letras-baixo", "espacamento-letras-medio", "espacamento-letras-alto");

      if (this.espacamentoNivel === 1) document.body.classList.add("espacamento-letras-baixo");
      else if (this.espacamentoNivel === 2) document.body.classList.add("espacamento-letras-medio");
      else if (this.espacamentoNivel === 3) document.body.classList.add("espacamento-letras-alto");
    },
    toggleEspacamentoLinhas() {
      this.linhaNivel = (this.linhaNivel + 1) % 4;
      document.body.classList.remove("espacamento-linhas-baixo", "espacamento-linhas-medio", "espacamento-linhas-alto");

      if (this.linhaNivel === 1) document.body.classList.add("espacamento-linhas-baixo");
      else if (this.linhaNivel === 2) document.body.classList.add("espacamento-linhas-medio");
      else if (this.linhaNivel === 3) document.body.classList.add("espacamento-linhas-alto");
    }
  },
};
</script>

<style scoped>
.acessibilidade-container {
  position: fixed;
  right: 10px;
  bottom: 405px; /* distancia VLibras */
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  z-index: 9999;
}

.acessibilidade-botao {
  background-color: #3084ee;
  border: none;
  border-radius: 9px;
  padding: 4.3px;
  cursor: pointer;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.2);
  width: 40px;
  height: 40px;
  display: flex;
  justify-content: center;
  align-items: center;
}

.icone {
  background-color: #3084ee;
  width: 32px;
  height: 32px;
  width: 32px !important;
  height: 32px !important;
  object-fit: contain;
  pointer-events: none;
}

.acessibilidade-menu {
  position: absolute;
  right: 50px;
  top: 0;
  background: #ffffff;
  padding: 10px;
  border: 1px solid #ccc;
  border-radius: 6px;
  box-shadow: 0px 2px 8px rgba(0, 0, 0, 0.2);
  min-width: 420px;
  min-height: 320px;
  z-index: 10000;
}

.acessibilidade-menu button {
  display: block;
  width: 100%;
  margin-bottom: 8px;
  padding: 13px 10px;
  font-size: 20px;
  border: none;
  background-color: #f0f0f0;
  cursor: pointer;
  border-radius: 4px;
  text-align: left;
}

.icone-contraste {
  width: 35px;
  height: 35px;
  margin-right: 5px;
}

.icone-fonte {
  width: 35px;
  height: 35px;
  margin-right: 5px;
}

.icone-dislexia {
  width: 35px;
  height: 35px;
  margin-right: 5px;
}

.icone-letras {
  width: 35px;
  height: 35px;
  margin-right: 5px;
}

.icone-linhas {
  width: 35px;
  height: 35px;
  margin-right: 5px;
}
</style>
