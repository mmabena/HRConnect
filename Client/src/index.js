import React from "react";
import ReactDOM from "react-dom/client";
import "./styles/global.css";
import "./index.css";
import App from "./App";
import SignalRProvider from "./api/SignalRProvider";
import { BrowserRouter } from "react-router-dom";
import reportWebVitals from "./reportWebVitals";

const root = ReactDOM.createRoot(document.getElementById("root"));
root.render(
  <React.StrictMode>
    <BrowserRouter>
      <SignalRProvider>
        <App />
      </SignalRProvider>
    </BrowserRouter>
  </React.StrictMode>,
);

reportWebVitals();
