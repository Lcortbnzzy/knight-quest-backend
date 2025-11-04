import js from "@eslint/js";
import globals from "globals";
import tseslint from "typescript-eslint";
import { defineConfig } from "eslint/config";


export default defineConfig([
    js.configs.recommended,
    ...tseslint.configs.recommended,
    {
        files: [ "**/*.{js,mjs,cjs,ts,mts,cts}" ],
        languageOptions: {
            globals: globals.browser
        },
        rules: {
            "@typescript-eslint/no-unused-vars": "off",
            "@typescript-eslint/no-explicit-any": "off"
        }
    }
])