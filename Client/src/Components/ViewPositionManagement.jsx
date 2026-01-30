import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import "../MenuBar.css";

const ViewPositionManagement = () => {
  const { id } = useParams();
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    positionTitle: "",
    effectiveDate: "",
    jobGradeId: "",
    occupationalLevelId: "",
    occupationalLevel: "",
  });

  const [jobGrades, setJobGrades] = useState([]);
  const [occupationalLevels, setOccupationalLevels] = useState([]);

  useEffect(() => {
    if (!id) return;

    const fetchData = async () => {
      try {
        const [gradesRes, levelsRes] = await Promise.all([
          fetch("http://localhost:5037/api/JobGrades"),
          fetch("http://localhost:5037/api/OccupationalLevels"),
        ]);
        const [gradesData, levelsData] = await Promise.all([
          gradesRes.json(),
          levelsRes.json(),
        ]);
        setJobGrades(gradesData);
        setOccupationalLevels(levelsData);

        const posRes = await fetch(`http://localhost:5037/api/Positions/${id}`);
        if (!posRes.ok) throw new Error("Position not found");
        const posData = await posRes.json();

        const matchedLevel = levelsData.find(
          (level) => level.occupationalLevelId === posData.occupationalLevelId
        );

        setFormData({
          positionTitle: posData.positionTitle || "",
          effectiveDate: posData.createdDate
            ? new Date(posData.createdDate).toISOString().split("T")[0]
            : "",
          jobGradeId: posData.jobGradeId?.toString() || "",
          occupationalLevelId: posData.occupationalLevelId?.toString() || "",
          occupationalLevel: matchedLevel?.occupationalLevelName || "",
        });
      } catch (error) {
        console.error("Failed to fetch data:", error);
      }
    };

    fetchData();
  }, [id]);

  return (
    <div className="full-screen-bg">
      {/* Background shapes */}
      <div className="shape-1"></div>
      <div className="shape-2"></div>
      <div className="shape-3"></div>
      <div className="shape-4"></div>
      <div className="shape-5"></div>

      <div className="center-frame">
        <div className="left-frame">
          <div className="left-frame-centered">
            <div className="headings-container">
              <div className="apm-logo">
                <span className="apm-logo-bold">singular</span>
                <span className="apm-logo-light">express</span>
              </div>
              <h2 className="apm-title">Position Details</h2>
              <p className="apm-subtitle">
                View the information below for the selected
                <br />
                <span className="apm-highlight">Position Record.</span>
              </p>
            </div>

            <form className="apm-form">
              <div className="apm-input-group">
                <input
                  type="text"
                  name="positionTitle"
                  className="apm-input"
                  value={formData.positionTitle}
                  readOnly
                />
              </div>

              <div className="apm-input-group">
                <input
                  type="date"
                  name="effectiveDate"
                  className="apm-input"
                  value={formData.effectiveDate}
                  readOnly
                />
              </div>

              <div className="apm-input-group apm-dropdown-wrapper">
                <select
                  name="jobGradeId"
                  className="apm-input select-dropdown"
                  value={formData.jobGradeId}
                  disabled
                >
                  <option value="">Grade</option>
                  {jobGrades.map((grade) => (
                    <option key={grade.jobGradeId} value={grade.jobGradeId}>
                      {grade.jobGradeName}
                    </option>
                  ))}
                </select>
                <img
                  src="/images/arrow_drop_down_circle.png"
                  alt="Dropdown Icon"
                  className="apm-dropdown-icon"
                />
              </div>

              <div className="apm-input-group">
                <input
                  type="text"
                  name="occupationalLevel"
                  className="apm-input"
                  value={formData.occupationalLevel}
                  readOnly
                />
              </div>

              <button
                className="apm-save-button"
                onClick={(e) => {
                  e.preventDefault();
                  navigate("/positionManagement");
                }}
              >
                Back to Position Management Page
              </button>

              <div className="apm-footer">
                <p>
                  Privacy Policy &nbsp; | &nbsp; Terms & Conditions
                </p>
                <p>Copyright © 2025 Singular Systems. All rights reserved.</p>
              </div>
            </form>
          </div>
        </div>

        <div className="right-frame">
          <div className="apm-ellipse-wrapper">
            <div className="apm-ellipse-background"></div>
          </div>
          <div className="image-wrapper">
            <img
              src="/images/standing_man.svg"
              alt="Standing Man"
              className="center-image"
            />
          </div>
        </div>
      </div>
    </div>
  );
};

export default ViewPositionManagement;
