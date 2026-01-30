// src/components/TaxTableUpload.jsx
import React, { useState, useRef } from "react";
import axios from "axios";

import "../Navy.css";
//import "./TaxTableUpload.css";

const financialYears = [
  { value: "2022-2023", label: "March 2022 - April 2023" },
  { value: "2023-2024", label: "March 2023 - April 2024" },
  { value: "2024-2025", label: "March 2024 - April 2025" },
];

export default function TaxTableUpload() {
  const [year, setYear] = useState("");
  const [file, setFile] = useState(null);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [isUploading, setIsUploading] = useState(false);

  const fileInputRef = useRef(null);

  const handleYearChange = (e) => {
    setError("");
    setSuccess("");
    setYear(e.target.value);
  };

  const handleAutoUpload = async (e) => {
    setError("");
    setSuccess("");

    const selected = e.target.files[0];
    if (!selected) {
      setFile(null);
      return;
    }

    const ext = selected.name.split(".").pop().toLowerCase();
    if (!["xls", "xlsx"].includes(ext)) {
      setError("Only Excel files (.xls, .xlsx) are allowed.");
      setFile(null);
      fileInputRef.current.value = "";
      return;
    }

    if (!year) {
      setError("Please select a financial year before uploading.");
      fileInputRef.current.value = "";
      return;
    }

    setFile(selected);
    try {
      setIsUploading(true);
      const formData = new FormData();
      formData.append("year", year);
      formData.append("file", selected);

      const response = await axios.post(
        "http://localhost:5037/api/tax-tables/upload",
        formData,
        {
          headers: { "Content-Type": "multipart/form-data" },
          timeout: 10000,
        }
      );

      setSuccess(response.data.message || "Upload successful.");
      setYear("");
      setFile(null);
      fileInputRef.current.value = "";
    } catch (err) {
      console.error("[DEBUG] Auto-upload failed:", err);
      setError(err.response?.data?.message || "Upload failed.");
    } finally {
      setIsUploading(false);
    }
  };

  return (
    <div className="full-screen-bg tax-table-page">
      {/* Reusable background shapes */}
      <div className="shape-1" />
      <div className="shape-2" />
      <div className="shape-3" />
      <div className="shape-4" />
      <div className="shape-5" />

      <div className="tax-table-frame">
  <div className="tax-table-content-centered">
    <div className="tax-table-headings-container">
      <div className="tax-table-logo">
        <span className="tax-table-logo-text-bold">singular</span>
        <span className="tax-table-logo-text-light">express</span>
      </div>
            <h1 className="upload-title">Upload Tax Table</h1>
            <p className="file-type-text">Only Excel files (.xls, .xlsx) are supported</p>

            <div className="gender-select-wrapper">
              <select
                className="tax-name-input"
                value={year}
                onChange={handleYearChange}
              >
                <option value="" disabled>
                  Please select the year
                </option>
                {financialYears.map((yr) => (
                  <option key={yr.value} value={yr.value}>
                    {yr.label}
                  </option>
                ))}
              </select>
              <img
                className="dropdown-arrow"
                src="/images/arrow_drop_down_circle.png"
                alt="Dropdown Arrow"
              />
            </div>

            {error && <p className="error-message">{error}</p>}
            {success && <p className="success-message">{success}</p>}
          </div>

          <div className="upload-section">
            <div className="dashed-box">
              <p className="drop-files-text">Drop files here</p>
              <p className="or-text">or</p>
              <div className="upload-button-container">
                <label className="upload-file-button">
                  {isUploading ? "Uploading..." : "Upload file"}
                  <input
                    type="file"
                    accept=".xls,.xlsx"
                    ref={fileInputRef}
                    onChange={handleAutoUpload}
                    className="upload-hidden-input"
                  />
                </label>
              </div>
              {file && (
                <p className="selected-file-text">
                  Selected: <strong>{file.name}</strong>
                </p>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
