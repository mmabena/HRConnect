import { useState, useEffect } from "react";
import React from "react";
import api from "../../api/api";
import "./AddBenchmark.css";

function EditBenchmark({ benchmark, onClose, onEditSuccess }) {
  const [formData, setFormData] = useState({
    salary25th: benchmark.salary25th,
    salary50th: benchmark.salary50th,
    salary75th: benchmark.salary75th,
    source: benchmark.source,
    year: benchmark.year,
  });

  const [errors, setErrors] = useState({});
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);
  const [saving, setSaving] = useState(false);

  //- validations
  function validate() {
    const newErrors = {};
    if(!formData.year) newErrors.year = "Select year";
    if (!formData.salary25th || Number(formData.salary25th) <= 0)
      newErrors.salary25th = "Must be greater than 0";
    if (!formData.salary50th || Number(formData.salary50th) <= 0)
      newErrors.salary50th = "Must be greater than 0";
    if (!formData.salary75th || Number(formData.salary75th) <= 0)
      newErrors.salary75th = "Must be greater than 0";
    if (!formData.source.trim()) newErrors.source = "Required";
    if (Number(formData.salary25th) >= Number(formData.salary50th))
      newErrors.salary50th = "P50 must be greater than P25";
    if (Number(formData.salary50th) >= Number(formData.salary75th))
      newErrors.salary75th = "P75 must be greater than P50";


    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  }

  function handleChange(e) {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  }

  function handleReset() {
    // reset back to the original benchmark values, not empty
    setFormData({
      salary25th: benchmark.salary25th,
      salary50th: benchmark.salary50th,
      salary75th: benchmark.salary75th,
      source: benchmark.source,
    });
    setErrors({});
    setError(null);
  }

  async function handleSave() {
    if (saving) return;
    if (!validate()) return;

    try {
      setSaving(true);

      const payload = {
        Salary25th: Number(formData.salary25th),
        Salary50th: Number(formData.salary50th),
        Salary75th: Number(formData.salary75th),
        Source: formData.source.trim(),
        year: Number(formData.year),
      };

      const res = await api.put(`/salary-benchmarks/${benchmark.id}`, payload);

      setSuccess("Benchmark updated successfully!");
      onEditSuccess?.(res.data);
    } catch (err) {
      setError("Failed to update benchmark.");
    } finally {
      setSaving(false);
    }
  }

  useEffect(() => {
    if (success) {
      const timer = setTimeout(() => {
        setSuccess(null);
        onClose();
      }, 2000);
      return () => clearTimeout(timer);
    }
  }, [success]);

  const currentYear = new Date().getFullYear();

  const YEARS = Array.from(
    { length: currentYear - 2019 + 1,},
    (_, i) => currentYear - i
  );

  return (
    <div className="b-container">
      <div className="b-head-container">
        <button className="b-x-btn" onClick={onClose}>
          ✕
        </button>
        <div className="b-title-header">
          <span className="b-title">
            <svg
              width="34"
              height="34"
              viewBox="0 0 34 34"
              fill="none"
              xmlns="http://www.w3.org/2000/svg"
            >
              <rect
                width="34"
                height="34"
                rx="10"
                fill="white"
                fillOpacity="0.15"
              />
              <path
                d="M22 8L26 12M10 24L11.5 20L21 10.5L25 14.5L15.5 24L10 24Z"
                stroke="white"
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
              />
            </svg>
            Edit Market Salary Data
          </span>
        </div>
      </div>

      {error && <p className="error">{error}</p>}
      {success && <p className="success">{success}</p>}

      <div className="b-form">
        <div className="b-label-container">
          <div className="b-position-wrapper">
            <span className="b-label">POSITION TITLE</span>
            <div className="dropdown-wrapper">
              <input
                className="b-form-input-p"
                name="positionId"
                value={benchmark.positionTitle}
                disabled
              />
              <svg className="dropdown-icon" viewBox="0 0 20 20">
                <path d="M10 13L14 9H6L10 13Z" />
              </svg>
            </div>
          </div>
          <div className="b-date-wrapper">
            <span className="b-label-date">DATE</span>
            <div className="dropdown-wrapper-date">
              <select
                className="b-form-date-select"
                name="year"
                value={formData.year}
                onChange={handleChange}
              >
                <option value="">Select Year</option>

                {YEARS.map((year) => (
                  <option key={year} value={year}>
                    {year}
                  </option>
                ))}
              </select>
              {errors.year &&(
                <span className="error">{errors.year}</span>
              )}
            </div>

            <svg className="dropdown-icon" viewBox="0 0 20 20">
              <path d="M10 13L14 9H6L10 13Z" />
            </svg>
          </div>
        </div>
        <div className="b-location-card">
          <div className="b-location-header">
            <span className="b-location-card-title">{benchmark.location}</span>
            <p className="b-location-card-sub">South Africa Market Benchmark</p>
          </div>

          <div className="b-percentile-wrapper">
            <div className="b-percentile">
              <div className="b-percentile25">
                <span className="bp-label">25th Percentile(Lowest)</span>
                <div className="bp-source-wrapper">
                  <input
                    className="bp-form-input"
                    type="number"
                    name="salary25th"
                    value={formData.salary25th}
                    onChange={handleChange}
                  />
                  {errors.salary25th && (
                    <span className="error">{errors.salary25th}</span>
                  )}
                </div>
              </div>

              <div className="b-percentile50">
                <span className="bp-label">50th Percentile(Median)</span>
                <div className="bp-source-wrapper">
                  <input
                    className="bp-form-input"
                    type="number"
                    name="salary50th"
                    value={formData.salary50th}
                    onChange={handleChange}
                  />
                  {errors.salary50th && (
                    <span className="error">{errors.salary50th}</span>
                  )}
                </div>
              </div>

              <div className="b-percentile75">
                <span className="bp-label">75th Percentile(Highest)</span>
                <div className="bp-source-wrapper">
                  <input
                    className="bp-form-input"
                    type="number"
                    name="salary75th"
                    value={formData.salary75th}
                    onChange={handleChange}
                  />
                  {errors.salary75th && (
                    <span className="error">{errors.salary75th}</span>
                  )}
                </div>
              </div>
            </div>
          </div>

          <div className="b-source-container">
            <div className="b-label-wrapper">
              <span className="bp-label-s">SOURCE</span>
            </div>
            <div className="bp-source-wrapper">
              <input
                className="bp-source-box"
                type="text"
                name="source"
                value={formData.source}
                onChange={handleChange}
                placeholder="e.g. Payscale 2026"
              />
              {errors.source && <span className="error">{errors.source}</span>}
            </div>
          </div>
        </div>
      </div>
      <div className="b-buttons">
        <button className="b-reset" onClick={handleReset}>
          <svg
            width="20"
            height="20"
            viewBox="0 0 20 20"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              d="M2.5 10C2.5 11.4834 2.93987 12.9334 3.76398 14.1668C4.58809 15.4001 5.75943 16.3614 7.12987 16.9291C8.50032 17.4968 10.0083 17.6453 11.4632 17.3559C12.918 17.0665 14.2544 16.3522 15.3033 15.3033C16.3522 14.2544 17.0665 12.918 17.3559 11.4632C17.6453 10.0083 17.4968 8.50032 16.9291 7.12987C16.3614 5.75943 15.4001 4.58809 14.1668 3.76398C12.9334 2.93987 11.4834 2.5 10 2.5C7.90329 2.50789 5.89081 3.32602 4.38333 4.78333L2.5 6.66667M2.5 6.66667V2.5M2.5 6.66667H6.66667"
              stroke="#355867"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          </svg>
          Reset
        </button>

        <button
          type="button"
          className="b-complete"
          onClick={handleSave}
          disabled={saving}
        >
          <svg
            width="20"
            height="20"
            viewBox="0 0 20 20"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              d="M16.6654 5L7.4987 14.1667L3.33203 10"
              stroke="white"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          </svg>
          {saving ? "Saving..." : "Save Changes"}
        </button>
      </div>
    </div>
  );
}

export default EditBenchmark;
