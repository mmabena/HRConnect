import { useState, useEffect } from "react";
import "./annual-editor.css";
import {
  updateLeaveType,
  previewEntitlementImpact,
} from "../../api/leaveTypeApi";
import { useNavigate } from "react-router-dom";

const AnnualLeaveEditor = ({
  leaveType,
  onSuccess,
  onClose,
  isEditing,
  setIsEditing,
}) => {
  const [activeTab, setActiveTab] = useState("groupA");
  const [editedRules, setEditedRules] = useState({});
  const [customRules, setCustomRules] = useState([]);
  const [message, setMessage] = useState("");
  const [messageType, setMessageType] = useState("");
  const [errorFields, setErrorFields] = useState({});
  const [isCheckingImpact, setIsCheckingImpact] = useState(false);
  const [affectedEmployees, setAffectedEmployees] = useState([]);
  const navigate = useNavigate();

  const buildGroupedRules = () => {
    return {
      groupA: leaveType.rules.filter((r) => r.groupKey === "GROUP_A"),
      senior: leaveType.rules.filter((r) => r.groupKey === "SENIOR"),
      executive: leaveType.rules.filter((r) => r.groupKey === "EXECUTIVE"),
    };
  };

  const grouped = buildGroupedRules();

  const baseRules =
    activeTab === "groupA"
      ? grouped.groupA
      : activeTab === "senior"
        ? grouped.senior
        : grouped.executive;

  const currentRules = [...baseRules, ...customRules];
  const getRuleKey = (rule) => {
    return (
      rule.id ??
      `${rule.groupKey}-${rule.minYearsService}-${rule.maxYearsService}`
    );
  };
  const handleFieldChange = (ruleKey, field, value) => {
    setEditedRules((prev) => ({
      ...prev,
      [ruleKey]: {
        ...prev[ruleKey],
        [field]: value,
      },
    }));
    setErrorFields((prev) => {
      const copy = { ...prev };
      delete copy[ruleKey];
      return copy;
    });
  };
  const handleAddRange = () => {
    const groupKey =
      activeTab === "groupA"
        ? "GROUP_A"
        : activeTab === "senior"
          ? "SENIOR"
          : "EXECUTIVE";

    setCustomRules((prev) => [
      ...prev,
      {
        id: "new-" + Date.now(),
        groupKey: groupKey,
        minYearsService: 0,
        maxYearsService: null,
        daysAllocated: 0,
      },
    ]);
  };
  const hasChanges =
    Object.keys(editedRules).length > 0 || customRules.length > 0;

  const showMessage = (text, type = "error") => {
    setMessage(text);
    setMessageType(type);
  };

  const handleRemoveRange = (id) => {
    setCustomRules((prev) => prev.filter((r) => r.id !== id));
  };
  const mapBackendError = (message) => {
    if (!message) return "Something went wrong";

    const msg = message.toLowerCase();

    if (msg.includes("daysallocated")) {
      return "Leave days must be greater than 0";
    }

    if (msg.includes("maxyearsservice") && msg.includes("less")) {
      return "Max years cannot be less than minimum years";
    }

    if (msg.includes("gap detected")) {
      return "Employement year ranges must have no gaps between them. (Min and Max years)";
    }

    if (msg.includes("duplicate")) {
      return "Duplicate year service ranges are not allowed";
    }

    return "Unable to save changes. Please check your inputs.";
  };
  const buildPayload = () => {
    const allRules = [...leaveType.rules, ...customRules];

    const finalRules = allRules.map((r) => {
      const ruleKey = getRuleKey(r);

      const edited = editedRules[ruleKey];

      const min =
        edited?.minYearsService !== undefined
          ? Number(edited.minYearsService)
          : r.minYearsService;

      const max =
        edited?.maxYearsService !== undefined
          ? edited.maxYearsService === ""
            ? null
            : Number(edited.maxYearsService)
          : r.maxYearsService;

      const days =
        edited?.daysAllocated !== undefined
          ? Number(edited.daysAllocated)
          : r.daysAllocated;

      return {
        groupKey: r.groupKey,
        minYearsService: Number(min.toFixed(2)),
        maxYearsService: max !== null ? Number(max.toFixed(2)) : null,
        daysAllocated: Number(days.toFixed(2)),
      };
    });

    return {
      name: leaveType.name,
      description: leaveType.description,
      femaleOnly: leaveType.femaleOnly,
      isActive: leaveType.isActive,
      rules: finalRules,
    };
  };
  useEffect(() => {
    const checkImpact = async () => {
      if (!hasChanges) {
        setAffectedEmployees([]);
        return;
      }

      try {
        const payload = buildPayload();

        const previewData = await previewEntitlementImpact(payload);

        setAffectedEmployees(previewData || []);
      } catch (err) {
        console.error(err);
        setAffectedEmployees([]);
      }
    };

    checkImpact();
  }, [editedRules, customRules]);
  return (
    <div className="annual-wrapper">
      {message && (
        <div className={`message-text ${messageType}`}>{message}</div>
      )}
      <div className="tabs">
        <button
          className={activeTab === "groupA" ? "active" : ""}
          onClick={() => setActiveTab("groupA")}
        >
          Unskilled-Middle
        </button>

        <button
          className={activeTab === "senior" ? "active" : ""}
          onClick={() => setActiveTab("senior")}
        >
          Senior
        </button>

        <button
          className={activeTab === "executive" ? "active" : ""}
          onClick={() => setActiveTab("executive")}
        >
          Executive
        </button>
      </div>

      <div className="rule-box">
        <div className="rule-header">
          <span>Min Years</span>
          <span>Max Years</span>
          <span>Leave Days</span>
        </div>

        <div className="rule-body">
          {currentRules.map((r, index) => {
            const ruleKey = getRuleKey(r);
            const edited = editedRules[ruleKey] || {};

            return (
              <div key={ruleKey} className="rule-row">
                <input
                  disabled={!isEditing}
                  value={
                    edited.minYearsService !== undefined
                      ? edited.minYearsService
                      : r.minYearsService
                  }
                  onChange={(e) =>
                    handleFieldChange(
                      ruleKey,
                      "minYearsService",
                      e.target.value,
                    )
                  }
                />

                <input
                  disabled={!isEditing}
                  value={
                    edited.maxYearsService !== undefined
                      ? edited.maxYearsService
                      : (r.maxYearsService ?? "")
                  }
                  onChange={(e) =>
                    handleFieldChange(
                      ruleKey,
                      "maxYearsService",
                      e.target.value,
                    )
                  }
                />

                <div className="days-edit">
                  <input
                    disabled={!isEditing}
                    value={
                      edited.daysAllocated !== undefined
                        ? edited.daysAllocated
                        : r.daysAllocated
                    }
                    onChange={(e) =>
                      handleFieldChange(
                        ruleKey,
                        "daysAllocated",
                        e.target.value,
                      )
                    }
                  />

                  {edited.daysAllocated &&
                    Number(edited.daysAllocated) !== r.daysAllocated && (
                      <span className="diff">
                        {r.daysAllocated} → {edited.daysAllocated}
                      </span>
                    )}

                  {isEditing && r.id?.startsWith("new-") && (
                    <button
                      className="remove-range"
                      onClick={() => handleRemoveRange(r.id)}
                    >
                      ✕
                    </button>
                  )}
                </div>
              </div>
            );
          })}
        </div>

        {isEditing && (
          <button className="add-range" onClick={handleAddRange}>
            + Add Range
          </button>
        )}
      </div>

      <div className="impact-box">
        {Object.keys(editedRules).length > 0
          ? "Changes detected. Employees will be recalculated."
          : "No changes yet"}
      </div>

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
              disabled={!hasChanges}
              onClick={async () => {
                setIsCheckingImpact(true);
                await new Promise((resolve) => setTimeout(resolve, 100));
                try {
                  const payload = buildPayload();

                  const previewData = await previewEntitlementImpact(payload);

                  if (affectedEmployees.length === 0) {
                    await updateLeaveType(leaveType.id, payload);

                    showMessage(
                      "Leave entitlement updated successfully",
                      "success",
                    );

                    setTimeout(() => {
                      onSuccess();
                      onClose();
                    }, 800);

                    return;
                  }

                  navigate("/affected-employees", {
                    state: {
                      employees: previewData,
                      payload,
                      leaveTypeId: leaveType.id,
                    },
                  });
                } catch (err) {
                  console.error(err);

                  let message = err.response?.data;

                  if (typeof message === "object") {
                    message = message.title || JSON.stringify(message);
                  }

                  const friendlyMessage = mapBackendError(message);

                  showMessage(friendlyMessage, "error");
                } finally {
                  setIsCheckingImpact(false);
                }
              }}
            >
              {affectedEmployees.length > 0
                ? "Next: View Affected Employees"
                : "Save Changes"}
            </button>
          </>
        )}
      </div>
    </div>
  );
};

export default AnnualLeaveEditor;
