import React, { useEffect, useRef } from 'react';
import { driver } from "driver.js";
import "driver.js/dist/driver.css";
import "./Tutorial.css";
import { useAuth } from '../../context/AuthContext';
import { getStepsForRole } from './tutorialSteps';
import { HelpCircle } from 'lucide-react';

export const TutorialManager: React.FC = () => {
    const { user } = useAuth();
    const driverObj = useRef<ReturnType<typeof driver>>(null);

    useEffect(() => {
        if (!user) return;

        const role = user.role || 'Employee';
        const steps = getStepsForRole(role);

        driverObj.current = driver({
            showProgress: true,
            progressText: '{{current}} / {{total}}',
            nextBtnText: 'Next \u203a',
            prevBtnText: '\u2039 Previous',
            steps: steps,
            popoverClass: 'driverjs-theme',
            animate: true,
            allowClose: true,
            onDestroyStarted: () => {
                // Remove the confirm dialog as users find it annoying/non-functional
                localStorage.setItem(`tutorial_seen_${user.username}`, 'true');
                driverObj.current?.destroy();
            }
        });

        // Check if user has seen tutorial
        const hasSeen = localStorage.getItem(`tutorial_seen_${user.username}`);
        if (!hasSeen) {
            // Small delay to ensure UI is rendered
            setTimeout(() => {
                driverObj.current?.drive();
            }, 1000);
        }

    }, [user]);

    const startTutorial = () => {
        driverObj.current?.drive();
    };

    if (!user) return null;

    return (
        <button
            className="btn-icon-premium"
            onClick={startTutorial}
            title="Start Tour"
        >
            <HelpCircle size={20} />
        </button>
    );
};
