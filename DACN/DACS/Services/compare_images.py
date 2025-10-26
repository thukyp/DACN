# compare_images.py
import sys
import os
from PIL import Image
import imagehash
import numpy as np
import cv2
from skimage.metrics import structural_similarity as ssim
import json

def avg_hash_distance(img1_path, img2_path):
    try:
        h1 = imagehash.average_hash(Image.open(img1_path))
        h2 = imagehash.average_hash(Image.open(img2_path))
        return abs(h1 - h2)
    except Exception:
        return float('inf')

def ssim_distance(img1_path, img2_path):
    try:
        a = cv2.imread(img1_path, cv2.IMREAD_GRAYSCALE)
        b = cv2.imread(img2_path, cv2.IMREAD_GRAYSCALE)
        if a is None or b is None:
            return 1.0
        h = 1000
        aw = int(a.shape[1] * h / max(a.shape))
        bw = int(b.shape[1] * h / max(b.shape))
        a_r = cv2.resize(a, (aw, h))
        b_r = cv2.resize(b, (bw, h))
        min_h = min(a_r.shape[0], b_r.shape[0])
        min_w = min(a_r.shape[1], b_r.shape[1])
        a_crop = a_r[0:min_h, 0:min_w]
        b_crop = b_r[0:min_h, 0:min_w]
        s = ssim(a_crop, b_crop)
        return 1.0 - float(s)
    except Exception:
        return 1.0

def histogram_distance(img1_path, img2_path):
    try:
        a = cv2.imread(img1_path)
        b = cv2.imread(img2_path)
        if a is None or b is None:
            return 1.0
        a_hsv = cv2.cvtColor(a, cv2.COLOR_BGR2HSV)
        b_hsv = cv2.cvtColor(b, cv2.COLOR_BGR2HSV)
        histA = cv2.calcHist([a_hsv], [0,1], None, [50,60], [0,180,0,256])
        histB = cv2.calcHist([b_hsv], [0,1], None, [50,60], [0,180,0,256])
        cv2.normalize(histA, histA)
        cv2.normalize(histB, histB)
        d = cv2.compareHist(histA, histB, cv2.HISTCMP_CHISQR)
        return float(d) / (1.0 + float(d))
    except Exception:
        return 1.0

def keypoint_match_distance(img1_path, img2_path):
    try:
        img1 = cv2.imread(img1_path, cv2.IMREAD_GRAYSCALE)
        img2 = cv2.imread(img2_path, cv2.IMREAD_GRAYSCALE)
        if img1 is None or img2 is None:
            return 1.0

        try:
            sift = cv2.SIFT_create()
            kp1, des1 = sift.detectAndCompute(img1, None)
            kp2, des2 = sift.detectAndCompute(img2, None)
            matcher = cv2.BFMatcher()
        except Exception:
            orb = cv2.ORB_create(nfeatures=1500)
            kp1, des1 = orb.detectAndCompute(img1, None)
            kp2, des2 = orb.detectAndCompute(img2, None)
            matcher = cv2.BFMatcher(cv2.NORM_HAMMING, crossCheck=False)

        if des1 is None or des2 is None or len(kp1) == 0 or len(kp2) == 0:
            return 1.0

        try:
            matches = matcher.knnMatch(des1, des2, k=2)
            good = []
            for m_n in matches:
                if len(m_n) != 2:
                    continue
                m, n = m_n
                if m.distance < 0.75 * n.distance:
                    good.append(m)
            good_matches = len(good)
        except Exception:
            raw = matcher.match(des1, des2)
            good_matches = len(sorted(raw, key=lambda x: x.distance)[:50])

        max_possible = max(len(kp1), len(kp2), 1)
        score = good_matches / max_possible
        return 1.0 - score
    except Exception:
        return 1.0

def compare_all(uploaded_image_path, dataset_folder):
    results = []
    max_hash = 64.0

    for filename in os.listdir(dataset_folder):
        if not filename.lower().endswith(('.jpg','.jpeg','.png','.bmp','webp')):
            continue
        sample_path = os.path.join(dataset_folder, filename)

        h_diff = avg_hash_distance(uploaded_image_path, sample_path)
        h_norm = min(h_diff / max_hash, 1.0)
        ssim_d = ssim_distance(uploaded_image_path, sample_path)
        hist_d = histogram_distance(uploaded_image_path, sample_path)
        kp_d = keypoint_match_distance(uploaded_image_path, sample_path)

        w_hash = 0.10
        w_ssim = 0.35
        w_hist = 0.15
        w_kp = 0.40

        combined = w_hash * h_norm + w_ssim * ssim_d + w_hist * hist_d + w_kp * kp_d

        results.append({
            'filename': filename,
            'combined': float(combined)
        })

    if not results:
        return None, []

    results_sorted = sorted(results, key=lambda x: x['combined'])
    best = results_sorted[0]

    # 🔥 THÊM PHẦN NÀY: nếu điểm quá thấp thì coi như "không tìm thấy"
    similarity_score = 1.0 - best['combined']  
    threshold = 0.3

    if similarity_score < threshold:
        return None, results_sorted
        
    return best['filename'], results_sorted

def main():
    if len(sys.argv) < 3:
        print("Usage: compare_images.py <uploaded_image> <dataset_folder>")
        sys.exit(1)
    uploaded = sys.argv[1]
    dataset = sys.argv[2]
    best, all_results = compare_all(uploaded, dataset)
    if best is None:
        print("None")
        return
    print(best)
    print("DETAILS:" + json.dumps(all_results[:5], ensure_ascii=False))

if __name__ == "__main__":
    main()
