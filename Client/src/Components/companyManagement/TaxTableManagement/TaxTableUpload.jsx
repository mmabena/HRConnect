import React, { useState, useRef, useMemo } from "react";
import api from "../../../api/api.js";
import { toast } from "react-toastify";

/* ---------- YEAR GENERATOR ---------- */
const generateFinancialYears = (existingYears = []) => {
  const currentDate = new Date();
  const currentYear = currentDate.getFullYear();
  const currentMonth = currentDate.getMonth(); // 0 = Jan

  // Financial year starts in March (month index 2)
  const currentFinancialStartYear =
    currentMonth >= 2 ? currentYear : currentYear - 1;

  const years = [];

  // Start from 2022 (adjust if needed)
  for (let start = 2024; start <= currentFinancialStartYear; start++) {
    const end = start + 1;
    const value = `${start}-${end}`;

    // Exclude years already used
    if (!existingYears.includes(value)) {
      years.push({
        value,
        label: `March ${start} - April ${end}`,
      });
    }
  }

  return years;
};

function TaxTableUpload({ onClose, onUploadSuccess, existingYears = [] }) {
  const [year, setYear] = useState("");
  const [file, setFile] = useState(null);
  const [isUploading, setIsUploading] = useState(false);
  const fileInputRef = useRef(null);

  /* ✅ Generate dropdown YEARS inside component */
  const financialYears = useMemo(() => {
    return generateFinancialYears(existingYears);
  }, [existingYears]);

  const handleAutoUpload = async (e) => {
    const selected = e.target.files[0];
    if (!selected) {
      setFile(null);
      return;
    }

    const ext = selected.name.split(".").pop().toLowerCase();
    if (!["xls", "xlsx"].includes(ext)) {
      toast.error("Only Excel files (.xls, .xlsx) are allowed.");
      setFile(null);
      fileInputRef.current.value = "";
      return;
    }

    if (!year) {
      toast.warning("Please select a financial year before uploading.");
      fileInputRef.current.value = "";
      return;
    }

    setFile(selected);

    try {
      setIsUploading(true);

      const formData = new FormData();
      formData.append("year", year);
      formData.append("file", selected);

      const response = await api.post(
        "/tax-tables/upload",
        formData,
        {
          headers: { "Content-Type": "multipart/form-data" },
        }
      );

      toast.success(response.data.message || "Upload successful.");

      setYear("");
      setFile(null);
      fileInputRef.current.value = "";

      if (onUploadSuccess) {
        onUploadSuccess();
      }

    } catch (err) {
      console.error("Upload failed:", err);
      toast.error(err.response?.data?.message || "Upload failed.");
    } finally {
      setIsUploading(false);
    }
  };

  return (
    <div className="tax-table-popup-container">
      <div className="tax-table-frame">
        <div className="tax-table-content-centered">

          <button
            className="close-btn"
            onClick={onClose}
            style={{ float: "right", cursor: "pointer" }}
          >
            ✕
          </button>

          <div className="tax-table-headings-container">
            <div className="tax-table-logo">
              <span className="tax-table-logo-text-bold">singular</span>
              <span className="tax-table-logo-text-light">express</span>
            </div>

            <h1 className="upload-title">Upload Tax Table</h1>
            <p className="file-type-text">
              Only Excel files (.xls, .xlsx) are supported
            </p>

            <div className="gender-select-wrapper">
              <select
                className="tax-name-input"
                value={year}
                onChange={(e) => setYear(e.target.value)}
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

export default TaxTableUpload;