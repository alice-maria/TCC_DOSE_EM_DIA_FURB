import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import "./assets/styles/global.css";
import vuetify from './plugins/vuetify'; // Verifique o caminho
import { loadFonts } from './plugins/webfontloader'; // Verifique o caminho

loadFonts();

const app = createApp(App);
app.use(router);
app.use(vuetify);
app.mount("#app");