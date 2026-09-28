using System;

namespace GK2RustyToolDisposal.Helpers;

internal static class StudyStateHelper
{
    internal static bool HasBeenStudied(string itemId, SurveyDef survey, KnowledgeSystem knowledge)
    {
        // A shared/group recipe or a default-unlocked survey cannot prove this exact item was studied.
        if (
            !RustyToolHelper.IsSupported(itemId)
            || survey == null
            || survey.needItems == null
            || knowledge?.oneTimeCompletedCrafts == null
        )
            return false;
        var ingredient = survey.SurveyedItem;
        return survey.id == "surv:" + itemId
            && survey.isOneTimeCraft
            && !survey.isScienceFuelCraft
            && !survey.surveyedAtStart
            && survey.craftsIn != null
            && survey.craftsIn.Contains("survey_wgo")
            && ingredient != null
            && ingredient.groupType == ItemGroup.None
            && string.Equals(ingredient.id, itemId, StringComparison.Ordinal)
            && knowledge.oneTimeCompletedCrafts.Contains(survey.id)
            && knowledge.IsSurveyCompleted(survey);
    }
}
