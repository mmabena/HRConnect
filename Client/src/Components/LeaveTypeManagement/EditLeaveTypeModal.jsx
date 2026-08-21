import { useState, useEffect } from "react";
import "./add-leave-type-modal.css";
import { updateLeaveType } from "../../api/leaveTypeApi";
import AnnualLeaveEditor from "./AnnualLeaveEditor";

const EditLeaveTypeModal = ({
  isOpen,
  onClose,
  leaveTypes,
  onSuccess,
  selectedId,
  isViewMode
}) => {

  const [selected, setSelected] = useState(null);
  const [isEditing, setIsEditing] = useState(false);
  const [errors, setErrors] = useState({});
  const [apiError, setApiError] = useState("");
 
  

  const [form, setForm] = useState({
    description: "",
    femaleOnly: false,
    days: ""
  });

useEffect(() => {
  setIsEditing(!isViewMode);
  setErrors({});
  setApiError("");
}, [isViewMode]);
  useEffect(() => {
    if (!selectedId) return;

    const lt = leaveTypes.find(x => x.id === Number(selectedId));
    setSelected(lt);
    setErrors({});
    setApiError("");

    if (lt && lt.code !== "AL") {
      const uniqueDays = [...new Set(lt.rules.map(r => r.daysAllocated))];

      setForm({
        description: lt.description || "",
        femaleOnly: lt.code === "ML" ? lt.femaleOnly : false,
        days: uniqueDays.length === 1 ? uniqueDays[0] : ""
      });
    }

  }, [selectedId, leaveTypes]);

  if (!isOpen) return null;

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;

    setForm({
      ...form,
      [name]: type === "checkbox" ? checked : value
    });
  };
   const hasChanges =
  selected &&
  (
    form.description !== (selected.description || "") ||
    (selected.code !== "AL" && Number(form.days) !== Number(selected.rules?.[0]?.daysAllocated)) ||
    (selected.code === "ML" && form.femaleOnly !== selected.femaleOnly)
  );
  const handleSubmit = async () => {
  try {
    const validationErrors = validate();

if (Object.keys(validationErrors).length > 0) {
  setErrors(validationErrors);
  return;
}

setErrors({});
setApiError("");
    console.log("PAYLOAD:", {
  name: selected.name,
  description: form.description,
  femaleOnly: form.femaleOnly,
  isActive: selected.isActive,
  rules:
    selected.code === "AL"
      ? selected.rules
      : [
          {
            groupKey: "ALL",
            minYearsService: 0,
            maxYearsService: null,
            daysAllocated: parseFloat(form.days)
          }
        ]
});

    await updateLeaveType(selected.id, {
      name: selected.name,
      description: form.description,
      femaleOnly: form.femaleOnly,
      isActive: selected.isActive,
      rules:
        selected.code === "AL"
          ? selected.rules
          : [
              {
                groupKey: "ALL",
                minYearsService: 0,
                maxYearsService: null,
                daysAllocated: parseFloat(form.days)
              }
            ]
    });

    onSuccess();
    onClose();

  } catch (err) {
  console.error(err);

  if (err.response?.data) {
    let message = err.response.data;

    if (typeof message === "object") {
      message = message.title || JSON.stringify(message);
    }

    message = message.toLowerCase();

    if (message.includes("code")) {
      setErrors(prev => ({ ...prev, code: "Code already exists" }));
    } 
    else if (message.includes("name")) {
      setErrors(prev => ({ ...prev, name: "Name already exists" }));
    } 
    else {
      setApiError(message);
    }
  } else {
    setApiError("Failed to update leave type");
  }
}
};
const validate = () => {
  const newErrors = {};

  if (!form.description.trim()) {
    newErrors.description = "Description is required";
  }

  if (selected?.code !== "AL") {
    if (!form.days || isNaN(form.days) || Number(form.days) <= 0) {
      newErrors.days = "Enter valid number of days";
    }
  }

  if (form.femaleOnly && selected?.code !== "ML") {
    newErrors.femaleOnly = "Only allowed for maternity leave";
  }

  return newErrors;
};

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-card" onClick={(e) => e.stopPropagation()}>

        <div className="logo-container">
          <span className="logo-bold">singular</span>
          <span className="logo-light">express</span>
        </div>

        <h2 className="modal-title">
  {isEditing
    ? `Edit ${selected?.name}`
    : `View ${selected?.name}`
  }
</h2>
        {selected && selected.code !== "AL" && (
  <>
    <input value={selected.name} disabled />
    <input value={selected.code} disabled />

    <input
      name="description"
      value={form.description}
      onChange={handleChange}
      placeholder="Description"
      disabled={!isEditing}
    />
    <span className="error-text">{errors.description || ""}</span>
    <input
      name="days"
      value={form.days}
      onChange={handleChange}
      placeholder="Leave Entitlement"
      disabled={!isEditing}
    />
    <span className="error-text">{errors.days || ""}</span>
    {selected?.code === "ML" && isEditing && (
      <>
        <label className="checkbox">
          <input
            type="checkbox"
            name="femaleOnly"
            checked={form.femaleOnly}
            onChange={handleChange}
          />
          <span className="female">Female Only</span>
        </label>
        <span className="error-text">{errors.femaleOnly || ""}</span>
      </>
    )}

    <div className="actions">
      {!isEditing ? (
        <>
          <button className="cancel" onClick={onClose}>
            Back
          </button>

          <button className="next" onClick={() => setIsEditing(true)}>
            Edit
          </button>
        </>
      ) : (
        <>
          <button className="cancel" onClick={onClose}>
            Cancel
          </button>

          <button
            className="next"
            onClick={handleSubmit}
            disabled={!hasChanges}
          >
            Save Changes
          </button>
        </>
      )}
      {apiError && <div className="api-error">{apiError}</div>}
    </div>
    <p className="right-frame-bottom-text">
          <span className="align-right">
            Privacy Policy <span className="pipe">|</span> Terms & Conditions
          </span>
          <br />
          <span className="align-left">
            Copyright © 2026 Singular Systems. All rights reserved.
          </span>
        </p>

  </>
)}

        {selected && selected.code === "AL" && (
          <AnnualLeaveEditor
            leaveType={selected}
            onSuccess={onSuccess}
            onClose={onClose}
            isEditing={isEditing}
            setIsEditing={setIsEditing}
          />
        )}

      </div>
    </div>
  );
};

export default EditLeaveTypeModal;