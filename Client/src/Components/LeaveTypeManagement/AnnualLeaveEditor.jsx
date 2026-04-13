import { useState } from "react";
import "./annual-editor.css";
import { updateLeaveRule } from "../../api/leaveTypeApi";

const AnnualLeaveEditor = ({ leaveType }) => {

  const [activeTab, setActiveTab] = useState("groupA");
  const [editedRules, setEditedRules] = useState({});
  const [customRules, setCustomRules] = useState([]);

  /* GROUP RULES PROPERLY */
  const buildGroupedRules = () => {

    const groupA = leaveType.rules.filter(r =>
      [2, 3, 4, 6].includes(r.jobGradeId)
    );

    const uniqueGroupA = Object.values(
      groupA.reduce((acc, rule) => {
        const key = `${rule.minYearsService}-${rule.maxYearsService}`;

        if (!acc[key]) {
          acc[key] = {
            ...rule,
            ruleIds: [rule.id]
          };
        } else {
          acc[key].ruleIds.push(rule.id);
        }

        return acc;
      }, {})
    );

    return {
      groupA: uniqueGroupA,
      senior: leaveType.rules.filter(r => r.jobGradeId === 5),
      executive: leaveType.rules.filter(r => r.jobGradeId === 1)
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

  /* HANDLE CHANGE */
  const handleChange = (ruleIds, value) => {

    const updates = {};

    ruleIds.forEach(id => {
      updates[id] = value;
    });

    setEditedRules(prev => ({
      ...prev,
      ...updates
    }));
  };

  /* ADD RANGE */
  const handleAddRange = () => {
    setCustomRules(prev => [
      ...prev,
      {
        id: "new-" + Date.now(),
        minYearsService: 0,
        maxYearsService: 0,
        daysAllocated: 0,
        ruleIds: []
      }
    ]);
  };
  const handleRemoveRange = (id) => {
  setCustomRules(prev => prev.filter(r => r.id !== id));
};

  /* SAVE */
  const handleSave = async () => {
    try {
      for (const ruleId in editedRules) {
        await updateLeaveRule(ruleId, Number(editedRules[ruleId]));
      }

      alert("Rules updated successfully");

    } catch (err) {
      console.error(err);
      alert("Failed to update rules");
    }
  };

  return (
    <div className="annual-wrapper">

      {/* TABS */}
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

      {/* RULE TABLE */}
      <div className="rule-box">

  <div className="rule-header">
    <span>Min Years</span>
    <span>Max Years</span>
    <span>Leave Days</span>
  </div>

  {/* SCROLLABLE BODY */}
  <div className="rule-body">

    {currentRules.map((r) => {
      const firstRuleId = r.ruleIds?.[0] || r.id;
      const newValue = editedRules[firstRuleId];

      return (
        <div key={r.id} className="rule-row">

          <input
            value={r.minYearsService}
            onChange={(e) => {
              r.minYearsService = e.target.value;
            }}
          />

          <input
            value={r.maxYearsService ?? ""}
            onChange={(e) => {
              r.maxYearsService = e.target.value;
            }}
          />

          <div className="days-edit">
            <input
              value={newValue ?? r.daysAllocated}
              onChange={(e) =>
                handleChange(r.ruleIds || [r.id], e.target.value)
              }
            />

            {newValue && Number(newValue) !== r.daysAllocated && (
              <span className="diff">
                {r.daysAllocated} → {newValue}
              </span>
            )}

            {/* REMOVE BUTTON (only for new ranges) */}
            {String(r.id).startsWith("new-") && (
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

  {/* ADD RANGE BUTTON */}
  <button className="add-range" onClick={handleAddRange}>
    + Add Range
  </button>

</div>

      {/* IMPACT */}
      <div className="impact-box">
        {Object.keys(editedRules).length > 0
          ? "Changes detected. Employees will be recalculated."
          : "No changes yet"}
      </div>

      {/* ACTIONS */}
      <div className="actions">
        <button className="cancel">
          Cancel
        </button>

        <button className="next" onClick={handleSave}>
          Next: View Affected Employees
        </button>
      </div>

    </div>
  );
};

export default AnnualLeaveEditor;