import axios from "axios";

const API_BASE = "http://localhost:5147/api/Pension";

const api = axios.create({
  baseURL: API_BASE,
});

 
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("authToken");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const getPensionOptions = () =>
  api.get("/options").then((res) => res.data);

export const createPensionOption = (option) =>
  api.post("/options", {
    contributionPercentage: option.contributionPercentage,
  }).then((res) => res.data);

export const updatePensionOption = (option) =>
  api.put("/options", {
    pensionOptionId: option.pensionOptionId,
    contributionPercentage: option.contributionPercentage,
  }).then((res) => res.data);

export const deleteAllPensionOptions = () =>
  api.delete("/options/delete-all").then((res) => res.data);

export const getPensionFunds = () =>
  api.get("/funds").then((res) => res.data);

export const addPensionFund = (fund) =>
  api.post("/funds", fund).then((res) => res.data);

export const deletePensionFund = (id) =>
  api.delete(`/funds/${id}`).then((res) => res.data);



