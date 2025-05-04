<template>
  <div class="acessibilidade-container">
    <!-- Botão de acessibilidade -->
    <button class="acessibilidade-botao" @click="toggleMenu">
      <img :src="iconeVlibra" alt="Acessibilidade" class="icone" />
    </button>

    <!-- Menu de acessibilidade -->
    <div v-if="menuAberto" class="acessibilidade-menu">
      <button @click="toggleContraste">Contraste alto</button>
      <button @click="aumentarFonte">Aumentar fonte</button>
      <button @click="toggleFonteDislexia">Dislexia</button>
      <button @click="toggleEspacamentoLetras">Espaçamento entre letras</button>
      <button @click="toggleEspacamentoLinhas">Espaçamento entre linhas</button>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      menuAberto: false,
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
      document.body.classList.toggle("fonte-grande");
    },
    toggleFonteDislexia() {
      document.body.classList.toggle("fonte-dislexia");
    },
    toggleEspacamentoLetras() {
      document.body.classList.toggle("espacamento-letras");
    },
    toggleEspacamentoLinhas() {
      document.body.classList.toggle("espacamento-linhas");
    },
  },
};
</script>

<style scoped>
.acessibilidade-container {
  position: fixed;
  right: 20px;
  bottom: 150px; /* altura alinhada com o botão do VLibras */
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  z-index: 9999;
}

.acessibilidade-botao {
  background-color: #3084ee;
  border: none;
  border-radius: 9px;
  padding: 8px;
  cursor: pointer;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.2);
}

.icone {
  width: 32px;
  height: 32px;
}

/* Menu flutuante à esquerda do botão */
.acessibilidade-menu {
  position: absolute;
  right: 50px;
  top: 0;
  background: #ffffff;
  padding: 10px;
  border: 1px solid #ccc;
  border-radius: 6px;
  box-shadow: 0px 2px 8px rgba(0, 0, 0, 0.2);
  min-width: 180px;
}

.acessibilidade-menu button {
  display: block;
  width: 100%;
  margin-bottom: 6px;
  padding: 6px 10px;
  font-size: 14px;
  border: none;
  background-color: #f0f0f0;
  cursor: pointer;
  border-radius: 4px;
  text-align: left;
}
</style>
