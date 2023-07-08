{\rtf1\ansi\ansicpg1252\cocoartf2707
\cocoatextscaling0\cocoaplatform0{\fonttbl\f0\fnil\fcharset0 Monaco;}
{\colortbl;\red255\green255\blue255;\red255\green255\blue255;\red0\green0\blue0;}
{\*\expandedcolortbl;;\cssrgb\c100000\c100000\c100000;\cssrgb\c0\c0\c0;}
\margl1440\margr1440\vieww28600\viewh15400\viewkind0
\deftab720
\pard\pardeftab720\partightenfactor0

\f0\fs28 \cf2 \cb3 \expnd0\expndtw0\kerning0
\outl0\strokewidth0 \strokec2 #import <Foundation/Foundation.h>\
#import <UIKit/UIKit.h>\
\
// Objective-C function to convert HEIC to PNG\
NSData* ConvertHEICtoPNG(const uint8_t* heicBytes, int heicLength) \{\
    NSData *heicData = [NSData dataWithBytes:heicBytes length:heicLength];\
    UIImage *heicImage = [UIImage imageWithData:heicData];\
\
    // Convert HEIC image to PNG representation\
    NSData *pngData = UIImagePNGRepresentation(heicImage);\
    \
    return pngData;\
\}}