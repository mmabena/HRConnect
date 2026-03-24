import { useState, useEffect } from "react";
import "../../MenuBar/MenuBar.css";
import { toast } from "react-toastify";
import { confirmAlert } from "react-confirm-alert";
import "react-confirm-alert/src/react-confirm-alert.css";
import "react-toastify/dist/ReactToastify.css";
import api from "../../../api/api";
import { jwtDecode } from "jwt-decode";
import { useNavigate } from "react-router-dom";

const EditPositionManagement = ({ id, isOpen, onClose, onOpenChangeModal })  => {
  const navigate = useNavigate();
  const [originalTitle, setOriginalTitle] = useState("");
  const [allPositions, setAllPositions] = useState([]);


  const [formData, setFormData] = useState({
    positionId: id,
    positionTitle: "",
    effectiveDate: "",
    jobGradeId: "",
    occupationalLevelId: "",
  });

  const [jobGrades, setJobGrades] = useState([]);
  const [occupationalLevels, setOccupationalLevels] = useState([]);
  const [loading, setLoading] = useState(true);
  const [hasAccess, setHasAccess] = useState(false);

  useEffect(() => {
     if (!id) return; 
    const initialize = async () => {
      const token = localStorage.getItem("token");
      if (!token) {
        setLoading(false);
        return;
      }

      try {
        const decoded = jwtDecode(token);
        const role =
          decoded?.role ||
          decoded?.[
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
          ];

      const isSuperUser = role === "SuperUser";
      setHasAccess(isSuperUser);

        const [gradesRes, levelsRes, positionRes, positionsRes] =
          await Promise.all([
            api.get("/jobgrades"),
            api.get("/occupationallevels"),
            api.get(`/positions/${id}`),
            api.get("/positions"),
          ]);

        setJobGrades(gradesRes.data);
        setOccupationalLevels(levelsRes.data);
        setAllPositions(positionsRes.data);

        const posData = positionRes.data;

        // Store original title from DB
        setOriginalTitle(posData.positionTitle);

        setFormData({
          positionId: posData.positionId,
          positionTitle: posData.positionTitle || "",
          effectiveDate: posData.createdDate
            ? new Date(posData.createdDate).toISOString().slice(0, 10)
            : "",
          jobGradeId: posData.jobGradeId?.toString() || "",
          occupationalLevelId: posData.occupationalLevelId?.toString() || "",
        });
      } catch (error) {
        console.error(error);
        toast.error("Failed to load position data.");
      } finally {
        setLoading(false);
      }
    };

    initialize();
  }, [id]);

  const handleChange = (e) => {
    const { name, value } = e.target;

    if (name === "jobGradeId" && value !== formData.jobGradeId) {
      confirmAlert({
        title: "Confirm Change",
        message: "Are you sure you want to change the Job Grade?",
        buttons: [
          {
            label: "Yes",
            onClick: () =>
              setFormData((prev) => ({ ...prev, jobGradeId: value })),
          },
          { label: "No" },
        ],
      });
      return;
    }

    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    const { positionTitle, jobGradeId, occupationalLevelId } = formData;

    if (!positionTitle || !jobGradeId || !occupationalLevelId) {
      toast.error("All fields are required");
      return;
    }

    try {
      const employeesRes = await api.get("/employee");
      const linkedEmployees = employeesRes.data.filter(
        (emp) => emp.positionId === parseInt(id),
      );

      const duplicateTitle = allPositions.find(
        (pos) =>
          pos.positionTitle.toLowerCase() === positionTitle.toLowerCase() &&
          pos.positionId !== parseInt(id),
      );

   if (linkedEmployees.length > 0 || duplicateTitle) {
  if (onOpenChangeModal) {
    onOpenChangeModal({
      currentPosition: originalTitle,
            linkedEmployeesCount: linkedEmployees.length,
            attemptedTitle: positionTitle,
    });
    onClose(); // Close the edit modal
  }
  return; // Stop further submission
}

      await api.put(`/positions/${id}`, {
        positionTitle,
        jobGradeId: parseInt(jobGradeId),
        occupationalLevelId: parseInt(occupationalLevelId),
        isActive: true,
      });

      toast.success("Position updated successfully.");
      navigate("/positionManagement");
    } catch (error) {
      console.error(error);
      toast.error("Something went wrong.");
    }
  };

  if (loading) return <h3>Loading...</h3>;
  if (!hasAccess) return <h2>Access Denied. SuperUser only.</h2>;

  /*A FUNCTION FOR THE DATE */
  const formatDate = (date) => {
    if(!date) return "";

    return new Date(date).toLocaleDateString("en-GB", {
      day: "numeric",
      month: "long",
      year: "numeric"
    });
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content-edit" onClick={(e) => e.stopPropagation()}>
        <div className="headings-container">
          <div className="apm-logo">
            <span className="apm-logo-bold">singular</span>
            <span className="apm-logo-light">express</span>
          </div>
          <h2 className="pm-title-edit">Edit Position</h2>
        </div>

        <form onSubmit={handleSubmit} className="pm-form-edit">
          {/* Position Dropdown WITH icon */}
            <label className="title-placeholder-edit">Position title</label>
          <div className="pm-input-group-edit pm-dropdown-wrapper-edit">
          
            <select
              name="positionTitle"
              className="pm-input-edit "
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
     <label className="title-placeholder-edit">Position Grade</label>
          <div className="pm-input-group-edit pm-dropdown-wrapper-edit">
       
            <select
              name="jobGradeId"
              className="pm-input-edit "
              value={formData.jobGradeId}
              onChange={handleChange}
              required
            >
              <option value="">Position Grade</option>
              {jobGrades.map((grade) => (
                <option key={grade.jobGradeId} value={grade.jobGradeId}>
                  {grade.name}
                </option>
              ))}
            </select>

            <img
              src="/images/arrow_drop_down_circle.png"
              alt="Dropdown Icon"
              className="apm-dropdown-icon"
            />
          </div>
  <label className="title-placeholder-edit">
              Occupational Description
            </label>
          <div className="pm-input-group-edit pm-dropdown-wrapper-edit">
          
            <select
              name="occupationalLevelId"
              className="pm-input-edit"
              value={formData.occupationalLevelId}
              onChange={handleChange}
              required
            >
              <option value="">Occupational Level</option>
              {occupationalLevels.map((level) => (
                <option
                  key={level.occupationalLevelId}
                  value={level.occupationalLevelId}
                >
                  {level.description}
                </option>
              ))}
            </select>

            <img
              src="/images/arrow_drop_down_circle.png"
              alt="Dropdown Icon"
              className="apm-dropdown-icon"
            />
          </div>

<div className="pm-input-group-edit ">
  <label className="title-placeholder-edit">
    Effective Date
  </label>

  <div className="date-input-container">
    <input
      type="date"
      name="effectiveDate"
      className="apm-input date-input"
      value={formData.effectiveDate}
      onChange={handleChange}
      required
    />

    {formData.effectiveDate && (
      <span className="formatted-date-inside">
        {formatDate(formData.effectiveDate)}
      </span>
    )}

    <img
      src="/images/date-picker-removebg-preview.png"
      alt="Date picker"
      className="date-picker-icon"
    />
  </div>
</div>

          <button type="submit" className="apm-save-button">
            Save
          </button>
          <div className="pm-footer-edit">
            <p className="footer1">
              Privacy Policy &nbsp; | &nbsp; Terms & Conditions
            </p>
            <p>Copyright © 2026 Singular Systems. All rights reserved.</p>
          </div>
        </form>
      </div>
    </div>
  );
};

export default EditPositionManagement;
