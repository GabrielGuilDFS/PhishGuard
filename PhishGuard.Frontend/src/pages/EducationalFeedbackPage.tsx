import { useSearchParams } from 'react-router-dom';
import FeedbackTraining from '../components/FeedbackTraining';
import { educationalTemplates } from '../data/educationalTemplates';
import { feedbackTrainings } from '../data/feedbackTrainings';
import { EDU_FEEDBACK_QUERY } from '../shared/trackingContract';

export default function EducationalFeedbackPage() {
  const [searchParams] = useSearchParams();
  const templateId = searchParams.get(EDU_FEEDBACK_QUERY.template) || 'basico_phishing';
  const training = feedbackTrainings[templateId];
  if (training) return <FeedbackTraining config={training} />;

  const template = educationalTemplates.find((item) => item.id === templateId);
  const html = template?.html
    ?? '<div style="padding:2rem;text-align:center;font-family:sans-serif;color:#b00;">Treinamento não encontrado.</div>';
  return <div dangerouslySetInnerHTML={{ __html: html }} />;
}
