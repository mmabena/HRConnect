import React, { useState, useEffect } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import api from "../../../api/api";
import { toast } from "react-toastify";
import "../../MenuBar/MenuBar.css";
import "../../../Pages/CompanyManagement/PositionManagement/PositionManagement.css";

const ChangePositionManagement = () => {
  const location = useLocation();
  const navigate = useNavigate();

  const { currentPosition, linkedEmployeesCount, attemptedTitle } =
    location.state || {};

  const [moveUsers, setMoveUsers] = useState(false);

  const [formData, setFormData] = useState({
    positionTitle: attemptedTitle || "",
  });

  const [allPositions, setAllPositions] = useState([]);

  // Redirect if no position passed
  useEffect(() => {
    if (!currentPosition) {
      navigate("/positionManagement");
    }
  }, [currentPosition, navigate]);

  // Fetch all positions for dropdown
  useEffect(() => {
    const fetchPositions = async () => {
      try {
        const response = await api.get("/positions");
        setAllPositions(response.data);
      } catch (error) {
        console.error("Failed to fetch positions:", error);
        toast.error("Could not load positions.");
      }
    };

    fetchPositions();
  }, []);

  // Handle dropdown change
  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  // Handle submit
  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      await api.put(`/positions/change/${currentPosition}`, {
        newTitle: formData.positionTitle,
        moveUsers,
      });

      toast.success("Position updated successfully.");
      navigate("/positionManagement");
    } catch (error) {
      console.error("Update failed:", error);
      toast.error("Failed to update position.");
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <div className="headings-container">
          <div className="apm-logo">
            <span className="apm-logo-bold">singular</span>
            <span className="apm-logo-light">express</span>
          </div>
          <h2 className="apm-title">Change Position</h2>
        </div>
             <label className="title-placeholder">Position title</label>

        {/* Position Dropdown */}
        <div className="apm-input-group apm-dropdown-wrapper">
     
          <select
            name="positionTitle"
            className="apm-input select-dropdown"
            value={formData.positionTitle}
            onChange={handleChange}
            required
          >
            <option value="">Select Position</option>
            {allPositions.map((pos) => (
              <option key={pos.positionId} value={pos.positionTitle}>
                {pos.positionTitle}
              </option>
            ))}
          </select>

          <img
            src="/images/arrow_drop_down_circle.png"
            alt="Dropdown Icon"
            className="apm-dropdown-icon"
          />
        </div>
        <div className= "apm-info-text">
        <p >
          This position currently has{" "}
          <b>{linkedEmployeesCount} users</b>  assigned.
        </p>
        </div>
          <p className="Question"><b>What would you like to do?</b></p>
        <form onSubmit={handleSubmit} className="apm-form">
          {/* Keep Users */}
          <div className="checkbox-cell-position">
            <input
              type="checkbox"
              checked={!moveUsers}
              onChange={() => setMoveUsers(false)}
            />
            Keep users in current position
          </div>

          {/* Move Users */}
          <div className="checkbox-cell-position">
            <input
              type="checkbox"
              checked={moveUsers}
              onChange={() => setMoveUsers(true)}
            />
            Move users to a new position
          </div>

          {/* Conditional Link */}
        <div className={`apm-view-users ${moveUsers ? "show" : "hide"}`}>
  <span
    className="apm-link"
    onClick={() =>
      navigate(`/manageUserPosition`, {
        state: {
          currentPositionTitle: currentPosition,
          newPositionTitle: formData.positionTitle,
        },
      })
    }
  >
  <b>  View Users List &gt;</b>
  </span>
</div>

          <button type="submit" className="apm-save-button">
            Save
          </button>

          <div className="apm-footer">
            <p>Privacy Policy &nbsp; | &nbsp; Terms & Conditions</p>
            <p>
              Copyright © 2026 Singular Systems. All rights reserved.
            </p>
          </div>
        </form>
      </div>
    </div>
  );
};

export default ChangePositionManagement;