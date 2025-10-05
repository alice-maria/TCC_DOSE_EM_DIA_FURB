// src/plugins/vuetify.js
import 'vuetify/styles';
import { createVuetify } from 'vuetify';
import * as components from 'vuetify/components';
import * as directives from 'vuetify/directives';
import '@mdi/font/css/materialdesignicons.css';
import { VDatePicker } from 'vuetify/labs/VDatePicker'

export default createVuetify({
  components: {
    ...components, 
    VDatePicker,   
  },
  directives,
  icons: {
    defaultSet: 'mdi',
  },
})
